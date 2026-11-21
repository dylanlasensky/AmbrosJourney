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
  const buildScript = development ? script.replace(/game\.(data|wasm|framework\.js)\.gz/g, 'game.$1') : script;
  vm.runInNewContext(buildScript, {
    document: {
      getElementById: id => elements[id], createElement: () => ({}),
      body: { appendChild: element => { loader = element; } },
    },
    location: { reload: () => { reloads++; } }, console: { error() {}, warn() {}, log() {} },
    createUnityInstance: (canvas, config, progress) => {
      assert.equal(canvas, elements['unity-canvas']);
      configuration = config;
      progress(0.65);
      return pendingBuild ? new Promise(() => {}) : rejectBuild ? Promise.reject(new Error('fixture loading failure')) : Promise.resolve({});
    },
  });
  return { elements, get loader() { return loader; }, get configuration() { return configuration; }, get reloads() { return reloads; } };
}

async function run() {
  const loaded = fixture();
  assert.equal(loaded.loader.src, 'Build/game.loader.js');
  loaded.loader.onload();
  await Promise.resolve();
  await Promise.resolve();
  assert.equal(loaded.configuration.devicePixelRatio, 1, 'Retina screens do not multiply rendering pixels');
  assert.notEqual(loaded.configuration.matchWebGLToCanvasSize, false, 'Unity still resizes its backing buffer');
  for (const key of ['dataUrl', 'frameworkUrl', 'codeUrl', 'streamingAssetsUrl'])
    assert.ok(!loaded.configuration[key].startsWith('/'), `${key} works under an itch subdirectory`);
  assert.equal(loaded.elements['loading-progress'].value, 0.65);
  assert.equal(loaded.elements.loading.hidden, true);
  assert.equal(loaded.elements['unity-canvas'].focusCount, 1);
  loaded.elements['unity-canvas'].events.pointerdown();
  assert.equal(loaded.elements['unity-canvas'].focusCount, 2, 'Pointer input can restore keyboard focus');
  assert.match(template, /#unity-canvas\s*\{[^}]*width:\s*100%;[^}]*height:\s*100%/);

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
  console.log('PASS: Web template configuration, responsive CSS, focus, loading progress and error recovery');
}
module.exports = run();
if (require.main === module)
  module.exports.catch(error => { console.error(error); process.exitCode = 1; });
