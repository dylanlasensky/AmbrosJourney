const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

const template = fs.readFileSync(path.join(__dirname,
  '../capstone/Assets/WebGLTemplates/AmbrosJourney/index.html'), 'utf8');
const values = {
  WIDTH: '960', HEIGHT: '540', LOADER_FILENAME: 'game.loader.js', DATA_FILENAME: 'game.data.gz',
  FRAMEWORK_FILENAME: 'game.framework.js.gz', CODE_FILENAME: 'game.wasm.gz', SYMBOLS_FILENAME: 'game.symbols.json',
  'JSON.stringify(COMPANY_NAME)': '"test"', 'JSON.stringify(PRODUCT_NAME)': '"Ambros Journey"',
  'JSON.stringify(PRODUCT_VERSION)': '"1"',
};
const rendered = template.replace(/^#(?:if .*|endif)\r?\n/gm, '')
  .replace(/\{\{\{\s*(.*?)\s*\}\}\}/g, (_, name) => {
    assert.ok(Object.hasOwn(values, name), `Known Unity template variable: ${name}`);
    return values[name];
  });
const script = rendered.match(/<script>([\s\S]*?)<\/script>/)[1];

function fixture(rejectBuild = false, development = false, pendingBuild = false) {
  const elements = Object.fromEntries(['unity-canvas', 'loading', 'loading-progress', 'loading-message', 'retry']
    .map(id => [id, { hidden: id === 'retry', events: {}, focusCount: 0,
      addEventListener(name, action) { this.events[name] = action; }, focus() { this.focusCount++; } }]));
  let loader, configuration, reloads = 0;
  const documentState = {
    visibilityState: 'visible', activeElement: null,
    body: { appendChild: element => { loader = element; } }, documentElement: {},
  };
  const screen = { width: 960, height: 540, devicePixelRatio: 2 };
  const events = {}, documentEvents = {};
  const canvas = elements['unity-canvas'];
  canvas.getBoundingClientRect = () => ({width: screen.width, height: screen.height});
  canvas.focus = options => {
    assert.equal(options.preventScroll, true, 'Refocusing never scrolls the host page');
    canvas.focusCount++; documentState.activeElement = canvas;
  };
  let observer;
  const buildScript = development ? script.replace(/game\.(data|wasm|framework\.js)\.gz/g, 'game.$1') : script;
  vm.runInNewContext(buildScript, {
    document: Object.assign(documentState, {
      getElementById: id => elements[id], createElement: () => ({}),
      addEventListener: (name, action) => { documentEvents[name] = action; },
    }),
    window: { get devicePixelRatio() { return screen.devicePixelRatio; }, addEventListener: (name, action) => { events[name] = action; } },
    ResizeObserver: class { constructor(action) { observer = action; } observe(target) { assert.equal(target, canvas); } },
    location: { reload: () => { reloads++; } }, console: { error() {}, warn() {}, log() {} },
    createUnityInstance: (canvas, config, progress) => {
      assert.equal(canvas, elements['unity-canvas']);
      configuration = config;
      progress(0.65);
      return pendingBuild ? new Promise(() => {}) : rejectBuild ? Promise.reject(new Error('fixture loading failure')) : Promise.resolve({});
    },
  });
  return { elements, screen, events, documentEvents, documentState, get resizeObserver() { return observer; }, get loader() { return loader; }, get configuration() { return configuration; }, get reloads() { return reloads; } };
}

async function run() {
  const loaded = fixture();
  assert.equal(loaded.loader.src, 'Build/game.loader.js');
  assert.equal(loaded.elements['unity-canvas'].focusCount, 0, 'Initial buffer sizing does not take focus before loading');
  loaded.loader.onload();
  await Promise.resolve();
  await Promise.resolve();
  assert.equal(loaded.configuration.matchWebGLToCanvasSize, false, 'One resize policy owns the backing buffer');
  const canvas = loaded.elements['unity-canvas'];
  assert.equal(canvas.focusCount, 1, 'Successful loading initially focuses the game');
  loaded.resizeObserver(); loaded.events.resize(); loaded.documentEvents.fullscreenchange();
  assert.equal(canvas.focusCount, 1, 'Unchanged backing size never takes focus again');
  assert.deepEqual([canvas.width, canvas.height], [1920,1080], 'Retina embedded view renders sharp text at 2x');
  for (const [width,height,dpr] of [[1280,720,2],[1920,1080,2],[3840,2160,2],[720,1280,3],[1024,768,1],[960,540,1.25]]) {
    Object.assign(loaded.screen, {width,height,devicePixelRatio:dpr});
    loaded.resizeObserver();
    assert.ok(canvas.width * canvas.height <= 2560*1440, 'Large screens stay within the pixel budget');
    assert.ok(Math.abs(canvas.width/canvas.height - width/height) < .003, 'Aspect ratio survives fullscreen/portrait changes');
    if (dpr === 1) assert.deepEqual([canvas.width,canvas.height], [width,height], 'Standard-DPI laptop retains native resolution');
  }
  Object.assign(loaded.screen, {width:1280,height:720,devicePixelRatio:2});
  loaded.documentEvents.fullscreenchange();
  assert.deepEqual([canvas.width,canvas.height], [2560,1440], 'Fullscreen event refreshes backing size');
  Object.assign(loaded.screen, {width:960,height:540,devicePixelRatio:1});
  loaded.events.resize();
  assert.deepEqual([canvas.width,canvas.height], [960,540], 'Moving to a standard-DPI display refreshes density');
  Object.assign(loaded.screen, {width:0,height:0});
  loaded.resizeObserver();
  assert.deepEqual([canvas.width,canvas.height], [960,540], 'Hidden iframe cannot destroy the current video buffer');
  for (const key of ['dataUrl', 'frameworkUrl', 'codeUrl', 'streamingAssetsUrl'])
    assert.ok(!loaded.configuration[key].startsWith('/'), `${key} works under an itch subdirectory`);
  assert.equal(loaded.elements['loading-progress'].value, 0.65);
  assert.equal(loaded.elements.loading.hidden, true);
  const beforePointer = canvas.focusCount;
  canvas.events.pointerdown();
  assert.equal(canvas.focusCount, beforePointer + 1, 'Pointer input can restore keyboard focus');
  assert.match(template, /#unity-canvas\s*\{[^}]*width:\s*100%;[^}]*height:\s*100%/);

  // Model an outer iframe fullscreen button retaining top-level focus. The child
  // can report its canvas/body as active even though Unity has lost foreground focus.
  const focusCase = fixture();
  focusCase.loader.onload(); await Promise.resolve(); await Promise.resolve();
  const game = focusCase.elements['unity-canvas'];
  let expectedFocus = game.focusCount;
  const resize = () => { focusCase.screen.width += 11; focusCase.resizeObserver(); };
  for (const active of [game, focusCase.documentState.body, focusCase.documentState.documentElement, null]) {
    focusCase.documentState.activeElement = active; resize();
    assert.equal(game.focusCount, ++expectedFocus, 'Visible loaded game refocuses after its buffer is actually cleared');
    assert.equal(focusCase.documentState.activeElement, game);
  }
  for (const active of [{tagName:'INPUT'}, {tagName:'TEXTAREA'}, {tagName:'SELECT'}, {tagName:'BUTTON'},
    {tagName:'DIV', isContentEditable:true}, {tagName:'A'}, {tagName:'DIV', tabIndex:0}]) {
    focusCase.documentState.activeElement = active; resize();
    assert.equal(game.focusCount, expectedFocus, 'Canvas resize preserves another focused browser control');
    assert.equal(focusCase.documentState.activeElement, active);
  }
  focusCase.documentState.activeElement = game;
  focusCase.documentState.visibilityState = 'hidden'; resize();
  assert.equal(game.focusCount, expectedFocus, 'Resizing a hidden tab never refocuses it');
  focusCase.documentState.visibilityState = 'visible';
  focusCase.configuration.showBanner('Fixture fatal error', 'error'); resize();
  assert.equal(game.focusCount, expectedFocus, 'A game showing load recovery does not steal focus on resize');
  const stillLoading = fixture(false, false, true); stillLoading.loader.onload();
  stillLoading.screen.width = 1200; stillLoading.resizeObserver();
  assert.equal(stillLoading.elements['unity-canvas'].focusCount, 0, 'Pending loading cannot steal focus on resize');

  const qa = fixture(false, true);
  qa.loader.onload();
  await Promise.resolve();
  for (const key of ['dataUrl', 'frameworkUrl', 'codeUrl']) {
    assert.ok(loaded.configuration[key].endsWith('.gz'), `${key} accepts release gzip filenames`);
    assert.ok(!qa.configuration[key].endsWith('.gz'), `${key} accepts uncompressed development filenames`);
  }
  assert.equal(qa.elements.loading.hidden, true);

  // Unity reports some framework/data failures through showBanner without rejecting its promise.
  loaded.configuration.showBanner('Nonfatal MIME warning', 'warning');
  assert.equal(loaded.elements.loading.hidden, true);
  const stalled = fixture(false, false, true);
  stalled.loader.onload();
  assert.equal(stalled.elements.retry.hidden, true);
  stalled.configuration.showBanner('Framework download failed', 'error');
  assert.equal(stalled.elements.loading.hidden, false);
  assert.equal(stalled.elements.retry.hidden, false);
  assert.match(stalled.elements['loading-message'].textContent, /could not load/);

  const failed = fixture(true);
  failed.loader.onload();
  await Promise.resolve();
  await Promise.resolve();
  assert.equal(failed.elements.loading.hidden, false);
  assert.equal(failed.elements.retry.hidden, false);
  assert.equal(failed.elements['loading-progress'].hidden, true);
  assert.match(failed.elements['loading-message'].textContent, /could not load/);
  failed.elements.retry.events.click();
  assert.equal(failed.reloads, 1);
  const missingLoader = fixture();
  missingLoader.loader.onerror();
  assert.equal(missingLoader.elements.retry.hidden, false, 'A missing loader offers recovery');
  console.log('PASS: Web template configuration, bounded Retina resolution, fullscreen resize/refocus guards, loading progress and error recovery');
}
module.exports = run();
if (require.main === module)
  module.exports.catch(error => { console.error(error); process.exitCode = 1; });
