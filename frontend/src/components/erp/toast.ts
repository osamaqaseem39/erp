import Swal from "sweetalert2";

/**
 * The single notification API for ERP pages. Built on sweetalert2's toast mode (already a dependency)
 * so it can be swapped later without touching call sites. Toasts are for outcomes ("Customer saved"),
 * never for errors the user must act on; those stay on the page next to the fields.
 */
const Toast = Swal.mixin({
  toast: true,
  position: "top-end",
  showConfirmButton: false,
  showCloseButton: true,
  timer: 3500,
  timerProgressBar: true,
  didOpen: (el) => {
    // Announce to screen readers; pause while hovered so it can be read.
    el.setAttribute("role", "status");
    el.setAttribute("aria-live", "polite");
    el.addEventListener("mouseenter", Swal.stopTimer);
    el.addEventListener("mouseleave", Swal.resumeTimer);
  },
});

export const toast = {
  success: (title: string, text?: string) => void Toast.fire({ icon: "success", title, text }),
  info: (title: string, text?: string) => void Toast.fire({ icon: "info", title, text }),
  warning: (title: string, text?: string) => void Toast.fire({ icon: "warning", title, text, timer: 6000 }),
  error: (title: string, text?: string) => void Toast.fire({ icon: "error", title, text, timer: 8000 }),
};
