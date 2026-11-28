// Exercise the cinematic focus bridge against simulated browser focus events.
const fs=require('node:fs'),path=require('node:path'),vm=require('node:vm'),assert=require('node:assert/strict');
const source=fs.readFileSync(path.join(__dirname,'../capstone/Assets/Journey/Plugins/WebGL/JourneyCinematicFocus.jslib'),'utf8');
function boot(options={}) {
  let calls=0,scrollOption;
  const document={visibilityState:options.hidden?'hidden':'visible',activeElement:Object.hasOwn(options,'active')?options.active:{tagName:'BODY'}};
  const canvas={isConnected:options.connected!==false,focus:settings=>{
    calls++;scrollOption=settings.preventScroll;
    if(options.throws)throw new Error('Focus denied');
    if(!options.ignored)document.activeElement=canvas;
  }};
  if(options.canvasActive)document.activeElement=canvas;
  const context={document,Module:{canvas:options.missing?null:canvas},LibraryManager:{library:{}},mergeInto:Object.assign};
  // No window object: requesting window.focus would fail this fixture.
  vm.runInNewContext(source,context);
  return {call:()=>context.LibraryManager.library.AmbrosReturnCinematicFocus(),get calls(){return calls;},get preventScroll(){return scrollOption;}};
}
for(const options of [{},{active:null},{canvasActive:true}]) {
  // An outer fullscreen control may leave the iframe's active element null, BODY, or its prior canvas.
  const normal=boot(options);assert.equal(normal.call(),1);assert.equal(normal.calls,1);assert.equal(normal.preventScroll,true);
}
for(const options of [{hidden:true},{missing:true},{connected:false},{active:{tagName:'INPUT'}},{active:{tagName:'BUTTON'}},{active:{isContentEditable:true}}]) {
  const skipped=boot(options);assert.equal(skipped.call(),0);assert.equal(skipped.calls,0);
}
assert.equal(boot({throws:true}).call(),0);
assert.equal(boot({ignored:true}).call(),0);
console.log('PASS: visible cinematic focus restoration, hidden-page/control preservation and focus-denial fallback');
