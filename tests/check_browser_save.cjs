// Run with node tests/check_browser_save.cjs. The bridge runs unchanged inside a fresh VM per reload.
const fs = require('node:fs'), vm = require('node:vm'), assert = require('node:assert/strict');
const source = fs.readFileSync(require('node:path').join(__dirname,'../capstone/Assets/Journey/Plugins/WebGL/JourneyPersistence.jslib'), 'utf8');
const values = new Map(); let denied = false;
const storage = { getItem: k => { if (denied) throw Error('SecurityError'); return values.get(k) || null; },
  setItem: (k, v) => { if (denied) throw Error('QuotaExceededError'); values.set(k, v); } };
function boot() {
  const memory = new Map(), listeners = { document: new Map(), window: new Map() }, calls = [];
  let next = 1;
  const target = name => ({ visibilityState: 'visible', addEventListener: (e, fn) => listeners[name].set(e, fn),
    removeEventListener: e => listeners[name].delete(e) });
  const context = { LibraryManager: { library: {} }, mergeInto: Object.assign, localStorage: storage,
    UTF8ToString: x => x, lengthBytesUTF8: x => Buffer.byteLength(x), _malloc: () => next++,
    stringToUTF8: (v, p) => memory.set(p, v), Module: {}, document: target('document'), window: target('window'),
    SendMessage: (...args) => calls.push(args) };
  vm.runInNewContext(source, context);
  const api = context.LibraryManager.library;
  return { api, read: (key, backup = 0) => memory.get(api.AmbrosReadSave(key, backup)), listeners, calls, context };
}
const key = 'ambros-journey:checkpoint:v1';
let first = boot();
const old = JSON.stringify({playerName:'Zoë',chapter:2,x:4.25,artifacts:[true,false]});
const current = JSON.stringify({playerName:'Zoë',chapter:4,x:108.25,inCave:true,artifacts:[true,true]});
assert.equal(first.api.AmbrosWriteSave(key, old), 1);
assert.equal(first.api.AmbrosWriteSave(key, current), 1);
let reloaded = boot();
assert.equal(reloaded.read(key), current); assert.equal(reloaded.read(key,1), old);
assert.equal(reloaded.read('other-game'), '');
denied = true;
assert.equal(reloaded.api.AmbrosWriteSave(key,'{}'),0); assert.equal(reloaded.read(key),'');
assert.equal(reloaded.api.AmbrosResetSave(key),0);
denied = false; assert.equal(boot().read(key),current);
assert.equal(reloaded.api.AmbrosResetSave(key),1); reloaded = boot();
assert.equal(reloaded.read(key),''); assert.equal(reloaded.read(key,1),''); assert.equal(reloaded.read(key,2),'reset');
values.set(key,'{bad'); assert.equal(boot().read(key),'');
assert.equal(reloaded.api.AmbrosWriteSave(key,old),1); assert.equal(boot().read(key),old);
values.set(key,'null'); assert.equal(reloaded.api.AmbrosWriteSave(key,current),1);
assert.equal(reloaded.api.AmbrosWriteSave(key,'x'.repeat(131073)),0);
reloaded.api.AmbrosInstallSaveHooks('Journey'); reloaded.api.AmbrosInstallSaveHooks('Journey');
assert.equal(reloaded.listeners.document.size,1); assert.equal(reloaded.listeners.window.size,1);
reloaded.context.document.visibilityState='hidden'; reloaded.listeners.document.get('visibilitychange')();
reloaded.listeners.window.get('pagehide')(); assert.equal(reloaded.calls.length,2);
assert.equal(reloaded.calls[0][1],'SaveBeforeBrowserHide');
console.log('PASS: browser reload, unicode checkpoint, recovery copy, atomic reset, corrupt storage, denied/quota storage, size bound and page-hide hooks.');
