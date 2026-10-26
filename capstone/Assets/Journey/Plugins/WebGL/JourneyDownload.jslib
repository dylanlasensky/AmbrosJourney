mergeInto(LibraryManager.library, {
  AmbrosDownloadPdf: function(pointer, length, filename) {
    var data = HEAPU8.slice(pointer, pointer + length);
    var url = URL.createObjectURL(new Blob([data], {type: 'application/pdf'}));
    var link = document.createElement('a'); link.href = url; link.download = UTF8ToString(filename);
    document.body.appendChild(link); link.click(); link.remove();
    setTimeout(function() { URL.revokeObjectURL(url); }, 60000);
  }
});
