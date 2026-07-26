mergeInto(LibraryManager.library, {
  WebGLVibrate: function (durationMilliseconds, pulseCount) {
    if (typeof navigator === "undefined" || !navigator.vibrate) {
      console.warn("[Haptics] navigator.vibrate is not available in this browser.");
      return 0;
    }

    try {
      var count = Math.max(1, pulseCount | 0);
      var pattern = [];
      for (var i = 0; i < count; i++) {
        if (i > 0) pattern.push(35);
        pattern.push(durationMilliseconds);
      }

      var activation = navigator.userActivation;
      var accepted = navigator.vibrate(pattern);
      console.log(
        "[Haptics] navigator.vibrate(" + JSON.stringify(pattern) + ")" +
        " accepted=" + accepted +
        " visible=" + (typeof document !== "undefined" ? document.visibilityState : "unknown") +
        " iframe=" + (window.self !== window.top) +
        " userActive=" + (activation ? activation.isActive : "unsupported") +
        " userActivatedBefore=" + (activation ? activation.hasBeenActive : "unsupported")
      );
      return accepted ? 1 : 0;
    } catch (error) {
      console.warn("[Haptics] Browser vibration failed:", error);
      return 0;
    }
  }
});
