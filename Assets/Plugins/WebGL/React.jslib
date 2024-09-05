mergeInto(LibraryManager.library, {
  FinishGame: function (jsonString) {
    window.dispatchReactUnityEvent("FinishGame", UTF8ToString(jsonString));
  },
ExitGame: function () {
    window.dispatchReactUnityEvent("ExitGame");
},
});