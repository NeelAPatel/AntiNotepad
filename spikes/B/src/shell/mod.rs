//! Windows tray and global-hotkey shell for the GPUI spike.
//!
//! The native message pump owns only a hidden message-only HWND, the tray icon,
//! and global hotkey registration. It sends [`ShellEvent`]s over an std channel;
//! the GPUI thread must call [`Shell::poll`] and apply them there.

use std::{
    fs::OpenOptions,
    io::Write,
    path::PathBuf,
    sync::{Mutex, OnceLock, mpsc},
    time::{Instant, SystemTime, UNIX_EPOCH},
};

#[derive(Debug, Clone, PartialEq, Eq)]
pub enum ShellEvent {
    Shown,
    Hidden,
    Quit,
    Error(String),
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub struct ShellError(pub String);

impl std::fmt::Display for ShellError {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        f.write_str(&self.0)
    }
}

impl std::error::Error for ShellError {}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
enum HotkeyChord {
    Primary,
    Fallback,
}

impl HotkeyChord {
    fn label(self) -> &'static str {
        match self {
            Self::Primary => "Ctrl+Alt+Space",
            Self::Fallback => "Ctrl+Alt+Shift+A",
        }
    }
}

fn choose_hotkey<E>(
    mut register: impl FnMut(HotkeyChord) -> Result<(), E>,
) -> Result<HotkeyChord, (E, E)> {
    match register(HotkeyChord::Primary) {
        Ok(()) => Ok(HotkeyChord::Primary),
        Err(primary_error) => match register(HotkeyChord::Fallback) {
            Ok(()) => Ok(HotkeyChord::Fallback),
            Err(fallback_error) => Err((primary_error, fallback_error)),
        },
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn primary_collision_selects_fallback() {
        let mut attempts = Vec::new();
        let selected = choose_hotkey(|chord| {
            attempts.push(chord);
            match chord {
                HotkeyChord::Primary => Err(1409),
                HotkeyChord::Fallback => Ok(()),
            }
        });

        assert_eq!(selected, Ok(HotkeyChord::Fallback));
        assert_eq!(attempts, [HotkeyChord::Primary, HotkeyChord::Fallback]);
    }

    #[test]
    fn reports_both_errors_when_no_chord_registers() {
        let result = choose_hotkey(|chord| match chord {
            HotkeyChord::Primary => Err("primary collision"),
            HotkeyChord::Fallback => Err("fallback collision"),
        });

        assert_eq!(result, Err(("primary collision", "fallback collision")));
    }
}

#[cfg(windows)]
mod platform {
    use super::*;
    use std::{
        ffi::c_void,
        mem::{size_of, zeroed},
        ptr::{null, null_mut},
        sync::Arc,
        sync::atomic::{AtomicBool, AtomicU64},
        sync::atomic::{AtomicIsize, Ordering},
        thread::{self, JoinHandle},
        time::Duration,
    };

    type Hwnd = *mut c_void;
    type Hinstance = *mut c_void;
    type Hicon = *mut c_void;
    type Hmenu = *mut c_void;
    type Hcursor = *mut c_void;
    type Hbrush = *mut c_void;
    type Bool = i32;
    type Uint = u32;
    type Wparam = usize;
    type Lparam = isize;
    type Lresult = isize;

    const WM_DESTROY: Uint = 0x0002;
    const WM_CLOSE: Uint = 0x0010;
    const WM_HOTKEY: Uint = 0x0312;
    const WM_APP: Uint = 0x8000;
    const WM_TRAY: Uint = WM_APP + 1;
    const WM_RBUTTONUP: usize = 0x0205;
    const WM_LBUTTONUP: usize = 0x0202;
    const MOD_ALT: Uint = 0x0001;
    const MOD_CONTROL: Uint = 0x0002;
    const MOD_SHIFT: Uint = 0x0004;
    const MOD_NOREPEAT: Uint = 0x4000;
    const VK_SPACE: Uint = 0x20;
    const VK_A: Uint = 0x41;
    const HOTKEY_ID: i32 = 1;
    const NIM_ADD: Uint = 0;
    const NIM_MODIFY: Uint = 1;
    const NIM_DELETE: Uint = 2;
    const NIF_MESSAGE: Uint = 1;
    const NIF_ICON: Uint = 2;
    const NIF_TIP: Uint = 4;
    const IDI_APPLICATION: *const u16 = 32512usize as *const u16;
    const SW_HIDE: i32 = 0;
    const SW_SHOW: i32 = 5;
    const GW_OWNER: Uint = 4;
    const GWLP_USERDATA: i32 = -21;
    const GPUI_WINDOW_CLASS: &str = "Zed::Window";
    const TPM_RIGHTBUTTON: Uint = 0x0002;
    const TPM_RETURNCMD: Uint = 0x0100;
    const MF_STRING: Uint = 0;
    const MF_SEPARATOR: Uint = 0x0800;
    const ID_SHOW: usize = 100;
    const ID_HIDE: usize = 101;
    const ID_QUIT: usize = 102;
    const SELF_CHECK_LPARAM: Lparam = isize::MIN;

    #[repr(C)]
    struct Point {
        x: i32,
        y: i32,
    }

    #[repr(C)]
    struct Msg {
        hwnd: Hwnd,
        message: Uint,
        wparam: Wparam,
        lparam: Lparam,
        time: Uint,
        pt: Point,
        private: Uint,
    }

    type WndProc = Option<unsafe extern "system" fn(Hwnd, Uint, Wparam, Lparam) -> Lresult>;

    #[repr(C)]
    struct WndClassW {
        style: Uint,
        wnd_proc: WndProc,
        cls_extra: i32,
        wnd_extra: i32,
        instance: Hinstance,
        icon: Hicon,
        cursor: Hcursor,
        background: Hbrush,
        menu_name: *const u16,
        class_name: *const u16,
    }

    #[repr(C)]
    #[derive(Clone, Copy)]
    struct Guid {
        data1: u32,
        data2: u16,
        data3: u16,
        data4: [u8; 8],
    }

    #[repr(C)]
    union NotifyTimeout {
        timeout: Uint,
        version: Uint,
    }

    // Full Vista+ NOTIFYICONDATAW layout (including alignment and trailing balloon fields).
    #[repr(C)]
    struct NotifyIconDataW {
        cb_size: Uint,
        hwnd: Hwnd,
        id: Uint,
        flags: Uint,
        callback_message: Uint,
        icon: Hicon,
        tip: [u16; 128],
        state: Uint,
        state_mask: Uint,
        info: [u16; 256],
        timeout_or_version: NotifyTimeout,
        info_title: [u16; 64],
        info_flags: Uint,
        guid_item: Guid,
        balloon_icon: Hicon,
    }

    #[link(name = "user32")]
    unsafe extern "system" {
        fn RegisterClassW(class: *const WndClassW) -> u16;
        fn CreateWindowExW(
            ex: Uint,
            class: *const u16,
            title: *const u16,
            style: Uint,
            x: i32,
            y: i32,
            width: i32,
            height: i32,
            parent: Hwnd,
            menu: Hmenu,
            instance: Hinstance,
            param: *mut c_void,
        ) -> Hwnd;
        fn DefWindowProcW(hwnd: Hwnd, msg: Uint, wparam: Wparam, lparam: Lparam) -> Lresult;
        fn DestroyWindow(hwnd: Hwnd) -> Bool;
        fn PostMessageW(hwnd: Hwnd, msg: Uint, wparam: Wparam, lparam: Lparam) -> Bool;
        fn GetMessageW(msg: *mut Msg, hwnd: Hwnd, min: Uint, max: Uint) -> Bool;
        fn TranslateMessage(msg: *const Msg) -> Bool;
        fn DispatchMessageW(msg: *const Msg) -> Lresult;
        fn PostQuitMessage(code: i32);
        fn RegisterHotKey(hwnd: Hwnd, id: i32, modifiers: Uint, key: Uint) -> Bool;
        fn UnregisterHotKey(hwnd: Hwnd, id: i32) -> Bool;
        fn SetForegroundWindow(hwnd: Hwnd) -> Bool;
        fn GetForegroundWindow() -> Hwnd;
        fn SetFocus(hwnd: Hwnd) -> Hwnd;
        fn ShowWindow(hwnd: Hwnd, command: i32) -> Bool;
        fn IsWindowVisible(hwnd: Hwnd) -> Bool;
        fn IsWindow(hwnd: Hwnd) -> Bool;
        fn GetWindow(hwnd: Hwnd, command: Uint) -> Hwnd;
        fn EnumWindows(
            callback: Option<unsafe extern "system" fn(Hwnd, Lparam) -> Bool>,
            param: Lparam,
        ) -> Bool;
        fn GetWindowThreadProcessId(hwnd: Hwnd, process_id: *mut Uint) -> Uint;
        fn GetWindowTextLengthW(hwnd: Hwnd) -> i32;
        fn GetWindowTextW(hwnd: Hwnd, text: *mut u16, max: i32) -> i32;
        fn GetClassNameW(hwnd: Hwnd, class: *mut u16, max: i32) -> i32;
        fn GetCursorPos(point: *mut Point) -> Bool;
        fn CreatePopupMenu() -> Hmenu;
        fn AppendMenuW(menu: Hmenu, flags: Uint, id: usize, text: *const u16) -> Bool;
        fn TrackPopupMenu(
            menu: Hmenu,
            flags: Uint,
            x: i32,
            y: i32,
            reserved: i32,
            hwnd: Hwnd,
            rect: *const c_void,
        ) -> Uint;
        fn DestroyMenu(menu: Hmenu) -> Bool;
        fn SetWindowLongPtrW(hwnd: Hwnd, index: i32, value: isize) -> isize;
        fn GetWindowLongPtrW(hwnd: Hwnd, index: i32) -> isize;
        fn GetModuleHandleW(name: *const u16) -> Hinstance;
        fn LoadIconW(instance: Hinstance, name: *const u16) -> Hicon;
    }

    #[link(name = "shell32")]
    unsafe extern "system" {
        fn Shell_NotifyIconW(message: Uint, data: *const NotifyIconDataW) -> Bool;
    }

    #[link(name = "kernel32")]
    unsafe extern "system" {
        fn GetCurrentProcessId() -> Uint;
    }

    #[link(name = "dwmapi")]
    unsafe extern "system" {
        fn DwmSetWindowAttribute(
            hwnd: Hwnd,
            attribute: Uint,
            value: *const c_void,
            size: Uint,
        ) -> i32;
    }

    fn wide(text: &str) -> Vec<u16> {
        text.encode_utf16().chain(Some(0)).collect()
    }

    fn epoch_micros() -> u64 {
        SystemTime::now()
            .duration_since(UNIX_EPOCH)
            .unwrap_or_default()
            .as_micros() as u64
    }

    fn log(log: &PathBuf, event: &str, detail: &str) {
        static LOCK: OnceLock<Mutex<()>> = OnceLock::new();
        let _guard = LOCK.get_or_init(|| Mutex::new(())).lock().ok();
        let micros = SystemTime::now()
            .duration_since(UNIX_EPOCH)
            .unwrap_or_default()
            .as_micros();
        match OpenOptions::new().create(true).append(true).open(log) {
            Ok(mut f) => {
                let _ = writeln!(f, "{micros}\t{event}\t{detail}");
            }
            Err(e) => eprintln!("shell log open failed ({}): {e}", log.display()),
        }
    }

    fn message_error(where_: &str) -> ShellError {
        let code = std::io::Error::last_os_error();
        ShellError(format!("{where_}: {code}"))
    }

    fn native_set_visible(shared: &NativeShared, show: bool) {
        let hwnd = shared.target_hwnd.load(Ordering::Acquire) as Hwnd;
        if hwnd.is_null() || unsafe { IsWindow(hwnd) == 0 } {
            let message =
                "hotkey or tray command arrived before exact-title window binding".to_owned();
            log(&shared.log_path, "SHELL_ERROR", &message);
            let _ = shared.events.send(ShellEvent::Error(message));
            return;
        }
        let started = Instant::now();
        if show {
            let foreground = unsafe { GetForegroundWindow() };
            if foreground != hwnd {
                shared
                    .previous_foreground
                    .store(foreground as isize, Ordering::Release);
            }
            unsafe {
                ShowWindow(hwnd, SW_SHOW);
                SetForegroundWindow(hwnd);
                SetFocus(hwnd);
            }
            shared.ready_logged.store(false, Ordering::Release);
            shared
                .show_started_us
                .store(epoch_micros(), Ordering::Release);
            log(
                &shared.log_path,
                "SHOW",
                &format!(
                    "native_show_and_foreground_request_us={}",
                    started.elapsed().as_micros()
                ),
            );
            let _ = shared.events.send(ShellEvent::Shown);
        } else {
            unsafe {
                ShowWindow(hwnd, SW_HIDE);
                let previous = shared.previous_foreground.load(Ordering::Acquire) as Hwnd;
                if !previous.is_null() && IsWindow(previous) != 0 {
                    SetForegroundWindow(previous);
                }
            }
            shared.show_started_us.store(0, Ordering::Release);
            log(
                &shared.log_path,
                "HIDE",
                &format!(
                    "native_hide_and_focus_restore_request_us={}\thidden_confirmed={}",
                    started.elapsed().as_micros(),
                    unsafe { IsWindowVisible(hwnd) == 0 }
                ),
            );
            let _ = shared.events.send(ShellEvent::Hidden);
        }
    }

    pub struct Shell {
        shared: Arc<NativeShared>,
        native_hwnd: AtomicIsize,
        title: String,
        log_path: PathBuf,
        rx: mpsc::Receiver<ShellEvent>,
        thread: Option<JoinHandle<()>>,
        active_hotkey: String,
    }

    struct NativeShared {
        target_hwnd: AtomicIsize,
        previous_foreground: AtomicIsize,
        show_started_us: AtomicU64,
        ready_logged: AtomicBool,
        events: mpsc::Sender<ShellEvent>,
        log_path: PathBuf,
        active_hotkey: Mutex<String>,
        self_check_ack: AtomicBool,
    }

    impl Shell {
        /// Start the Win32 tray/message thread and register the first available
        /// supported global hotkey.
        pub fn start(log_path: PathBuf) -> Result<Self, ShellError> {
            let (event_tx, rx) = mpsc::channel();
            let (startup_tx, startup_rx) = mpsc::sync_channel(1);
            let native_log = log_path.clone();
            let shared = Arc::new(NativeShared {
                target_hwnd: AtomicIsize::new(0),
                previous_foreground: AtomicIsize::new(0),
                show_started_us: AtomicU64::new(0),
                ready_logged: AtomicBool::new(false),
                events: event_tx,
                log_path: native_log.clone(),
                active_hotkey: Mutex::new(String::new()),
                self_check_ack: AtomicBool::new(false),
            });
            let native_shared = shared.clone();
            let thread = thread::Builder::new()
                .name("AntiQuickPad native shell".into())
                .spawn(move || native_thread(native_log, native_shared, startup_tx))
                .map_err(|e| ShellError(format!("start native shell thread: {e}")))?;
            let native_hwnd = match startup_rx.recv() {
                Ok(Ok((hwnd, active_hotkey))) => (hwnd, active_hotkey),
                Ok(Err(error)) => {
                    let _ = thread.join();
                    return Err(error);
                }
                Err(e) => {
                    let _ = thread.join();
                    return Err(ShellError(format!(
                        "native shell startup channel closed: {e}"
                    )));
                }
            };
            let (native_hwnd, active_hotkey) = native_hwnd;
            log(
                &log_path,
                "SHELL",
                &format!(
                    "tray_icon_registered=true\thotkey={active_hotkey}\thotkey_registered=true"
                ),
            );
            Ok(Self {
                shared,
                native_hwnd: AtomicIsize::new(native_hwnd as isize),
                title: String::new(),
                log_path,
                rx,
                thread: Some(thread),
                active_hotkey,
            })
        }

        /// The chord currently registered by the native shell.
        pub fn active_hotkey(&self) -> &str {
            &self.active_hotkey
        }

        /// Exercise native tray/hotkey startup and the posted WM_HOTKEY dispatch path.
        /// A missing interactive tray returns a distinct "self-check unavailable"
        /// error before hotkey registration. This does not verify physical delivery.
        pub fn self_check(log_path: PathBuf) -> Result<String, ShellError> {
            let mut shell = match Self::start(log_path) {
                Ok(shell) => shell,
                Err(error) if error.0.starts_with("Shell_NotifyIconW(NIM_ADD):") => {
                    return Err(ShellError(format!(
                        "self-check unavailable: tray registration failed before hotkey registration: {error}"
                    )));
                }
                Err(error) => return Err(error),
            };
            let hwnd = shell.native_hwnd.load(Ordering::Acquire) as Hwnd;
            let posted =
                unsafe { PostMessageW(hwnd, WM_HOTKEY, HOTKEY_ID as usize, SELF_CHECK_LPARAM) };
            if posted == 0 {
                let error = message_error("PostMessageW(WM_HOTKEY self-check probe)");
                shell.dispose();
                return Err(error);
            }
            let deadline = Instant::now() + Duration::from_secs(2);
            while !shell.shared.self_check_ack.load(Ordering::Acquire) && Instant::now() < deadline
            {
                thread::sleep(Duration::from_millis(5));
            }
            let passed = shell.shared.self_check_ack.load(Ordering::Acquire);
            let chord = shell.active_hotkey().to_owned();
            shell.dispose();
            if !passed {
                return Err(ShellError(
                    "posted WM_HOTKEY self-check probe was not dispatched within 2000ms".into(),
                ));
            }
            Ok(format!(
                "registered={chord}; posted WM_HOTKEY dispatch round-trip=passed; physical keystroke delivery=not tested"
            ))
        }

        /// Bind after GPUI creates its top-level window. Matching is constrained
        /// to this process, exact title, unowned top-level HWND, and a real window.
        pub fn bind(&mut self, title: &str) -> Result<(), ShellError> {
            self.title = title.to_owned();
            let mut query = FindWindowQuery {
                process: unsafe { GetCurrentProcessId() },
                title: wide(title),
                class: wide(GPUI_WINDOW_CLASS),
                found: null_mut(),
            };
            unsafe {
                EnumWindows(Some(find_exact_window), &mut query as *mut _ as Lparam);
            }
            if query.found.is_null() {
                log(
                    &self.log_path,
                    "SHELL_ERROR",
                    &format!(
                        "bind failed: no unowned {GPUI_WINDOW_CLASS} top-level window in this process with exact title {:?}",
                        title
                    ),
                );
                return Err(ShellError(format!(
                    "could not bind GPUI window with exact title {title:?}; call bind after open_window creates it"
                )));
            }
            self.shared
                .target_hwnd
                .store(query.found as isize, Ordering::Release);
            let started = Instant::now();
            unsafe {
                ShowWindow(query.found, SW_HIDE);
            }
            log(
                &self.log_path,
                "HIDE",
                &format!("initial_hide_request_us={}", started.elapsed().as_micros()),
            );
            Ok(())
        }

        /// Drain native messages. Call from the GPUI main-thread timer/tick.
        pub fn poll(&mut self) -> Vec<ShellEvent> {
            let mut events = Vec::new();
            while let Ok(event) = self.rx.try_recv() {
                events.push(event);
            }
            let started = self.shared.show_started_us.load(Ordering::Acquire);
            if started != 0 {
                let hwnd = self.shared.target_hwnd.load(Ordering::Acquire) as Hwnd;
                if !hwnd.is_null()
                    && unsafe { IsWindowVisible(hwnd) != 0 && GetForegroundWindow() == hwnd }
                    && !self.shared.ready_logged.swap(true, Ordering::AcqRel)
                {
                    let now = epoch_micros();
                    log(
                        &self.log_path,
                        "SHOW_READY",
                        &format!(
                            "show_to_visible_and_foreground_us={}",
                            now.saturating_sub(started)
                        ),
                    );
                    self.shared.show_started_us.store(0, Ordering::Release);
                } else if epoch_micros().saturating_sub(started) > 2_000_000 {
                    log(
                        &self.log_path,
                        "SHOW_TIMEOUT",
                        "window did not become visible and foreground within 2000ms",
                    );
                    self.shared.show_started_us.store(0, Ordering::Release);
                }
            }
            events
        }

        #[allow(dead_code)] // Available for app commands; hotkey/tray use the native dispatcher.
        pub fn toggle(&mut self) -> Result<(), ShellError> {
            let hwnd = self.window()?;
            native_set_visible(&self.shared, unsafe { IsWindowVisible(hwnd) == 0 });
            Ok(())
        }

        pub fn show(&mut self) -> Result<(), ShellError> {
            self.window()?;
            native_set_visible(&self.shared, true);
            Ok(())
        }

        pub fn hide(&mut self) -> Result<(), ShellError> {
            self.window()?;
            native_set_visible(&self.shared, false);
            Ok(())
        }

        /// Clear an earlier Mica request before switching material mechanisms.
        pub fn clear_backdrop(&mut self) {
            if let Ok(hwnd) = self.window() {
                let none = 1u32; // DWMSBT_NONE
                let result = unsafe {
                    DwmSetWindowAttribute(hwnd, 38, &none as *const _ as *const c_void, 4)
                };
                log(
                    &self.log_path,
                    "BACKDROP_RESET",
                    &format!("hresult={result}"),
                );
            }
        }

        fn window(&self) -> Result<Hwnd, ShellError> {
            let hwnd = self.shared.target_hwnd.load(Ordering::Acquire) as Hwnd;
            if hwnd.is_null() || unsafe { IsWindow(hwnd) == 0 } {
                Err(ShellError(format!(
                    "GPUI window is not bound or has closed (title {:?})",
                    self.title
                )))
            } else {
                Ok(hwnd)
            }
        }

        /// Remove tray/hotkey registrations and join the native message thread.
        pub fn dispose(&mut self) {
            let hwnd = self.native_hwnd.swap(0, Ordering::AcqRel) as Hwnd;
            // Windows requires a window to be destroyed by its creating thread.
            // WM_CLOSE is therefore posted to the tray thread's message queue.
            if !hwnd.is_null() {
                unsafe {
                    PostMessageW(hwnd, WM_CLOSE, 0, 0);
                }
            }
            if let Some(thread) = self.thread.take() {
                let _ = thread.join();
            }
            log(&self.log_path, "SHELL", "disposed=true");
        }
    }

    impl Drop for Shell {
        fn drop(&mut self) {
            self.dispose();
        }
    }

    struct FindWindowQuery {
        process: Uint,
        title: Vec<u16>,
        class: Vec<u16>,
        found: Hwnd,
    }

    unsafe extern "system" fn find_exact_window(hwnd: Hwnd, param: Lparam) -> Bool {
        let query = unsafe { &mut *(param as *mut FindWindowQuery) };
        if !query.found.is_null() || unsafe { GetWindow(hwnd, GW_OWNER).is_null() == false } {
            return 1;
        }
        let mut process = 0;
        unsafe {
            GetWindowThreadProcessId(hwnd, &mut process);
        }
        if process != query.process {
            return 1;
        }
        let len = unsafe { GetWindowTextLengthW(hwnd) };
        if len < 0 || len as usize + 1 != query.title.len() {
            return 1;
        }
        let mut title = vec![0u16; len as usize + 1];
        let copied = unsafe { GetWindowTextW(hwnd, title.as_mut_ptr(), title.len() as i32) };
        if copied != len || title != query.title {
            return 1;
        }
        let mut class = vec![0u16; query.class.len()];
        let copied_class = unsafe { GetClassNameW(hwnd, class.as_mut_ptr(), class.len() as i32) };
        if copied_class >= 0
            && copied_class as usize + 1 == query.class.len()
            && class == query.class
        {
            query.found = hwnd;
            return 0;
        }
        1
    }

    unsafe extern "system" fn wnd_proc(
        hwnd: Hwnd,
        msg: Uint,
        wparam: Wparam,
        lparam: Lparam,
    ) -> Lresult {
        match msg {
            WM_HOTKEY if wparam == HOTKEY_ID as usize => {
                let state =
                    unsafe { GetWindowLongPtrW(hwnd, GWLP_USERDATA) as *const NativeShared };
                if !state.is_null() {
                    let state = unsafe { &*state };
                    let chord = state
                        .active_hotkey
                        .lock()
                        .map(|s| s.clone())
                        .unwrap_or_default();
                    if lparam == SELF_CHECK_LPARAM {
                        state.self_check_ack.store(true, Ordering::Release);
                        log(
                            &state.log_path,
                            "HOTKEY",
                            &format!("origin=posted_self_check_probe\tchord={chord}"),
                        );
                        return 0;
                    }
                    log(
                        &state.log_path,
                        "HOTKEY",
                        &format!("origin=windows\tchord={chord}"),
                    );
                    let target = state.target_hwnd.load(Ordering::Acquire) as Hwnd;
                    native_set_visible(
                        state,
                        target.is_null() || unsafe { IsWindowVisible(target) == 0 },
                    );
                }
                0
            }
            WM_TRAY => {
                let state =
                    unsafe { GetWindowLongPtrW(hwnd, GWLP_USERDATA) as *const NativeShared };
                match lparam as usize {
                    WM_RBUTTONUP => unsafe {
                        show_tray_menu(hwnd, state);
                    },
                    WM_LBUTTONUP => {
                        if !state.is_null() {
                            let state = unsafe { &*state };
                            let target = state.target_hwnd.load(Ordering::Acquire) as Hwnd;
                            native_set_visible(
                                state,
                                target.is_null() || unsafe { IsWindowVisible(target) == 0 },
                            );
                        }
                    }
                    _ => {}
                }
                0
            }
            WM_CLOSE => {
                unsafe {
                    DestroyWindow(hwnd);
                }
                0
            }
            WM_DESTROY => {
                let state =
                    unsafe { GetWindowLongPtrW(hwnd, GWLP_USERDATA) as *const NativeShared };
                if !state.is_null() {
                    unsafe {
                        SetWindowLongPtrW(hwnd, GWLP_USERDATA, 0);
                        drop(Arc::from_raw(state));
                    }
                }
                unsafe {
                    PostQuitMessage(0);
                }
                0
            }
            _ => unsafe { DefWindowProcW(hwnd, msg, wparam, lparam) },
        }
    }

    unsafe fn show_tray_menu(hwnd: Hwnd, state: *const NativeShared) {
        if state.is_null() {
            return;
        }
        let menu = unsafe { CreatePopupMenu() };
        if menu.is_null() {
            return;
        }
        let show = wide("Show");
        let hide = wide("Hide");
        let quit = wide("Quit");
        unsafe {
            AppendMenuW(menu, MF_STRING, ID_SHOW, show.as_ptr());
            AppendMenuW(menu, MF_STRING, ID_HIDE, hide.as_ptr());
            AppendMenuW(menu, MF_SEPARATOR, 0, null());
            AppendMenuW(menu, MF_STRING, ID_QUIT, quit.as_ptr());
            let mut point = Point { x: 0, y: 0 };
            GetCursorPos(&mut point);
            SetForegroundWindow(hwnd);
            let selected = TrackPopupMenu(
                menu,
                TPM_RIGHTBUTTON | TPM_RETURNCMD,
                point.x,
                point.y,
                0,
                hwnd,
                null(),
            );
            let shared = &*state;
            match selected as usize {
                ID_SHOW => native_set_visible(shared, true),
                ID_HIDE => native_set_visible(shared, false),
                ID_QUIT => {
                    let _ = shared.events.send(ShellEvent::Quit);
                }
                _ => {}
            }
            DestroyMenu(menu);
        }
    }

    fn tray_data(hwnd: Hwnd, hotkey: &str) -> NotifyIconDataW {
        let mut tip = [0u16; 128];
        let text = wide(&format!("AntiQuickPad — {hotkey}"));
        tip[..text.len().min(128)].copy_from_slice(&text[..text.len().min(128)]);
        NotifyIconDataW {
            cb_size: size_of::<NotifyIconDataW>() as Uint,
            hwnd,
            id: 1,
            flags: NIF_MESSAGE | NIF_ICON | NIF_TIP,
            callback_message: WM_TRAY,
            icon: null_mut(),
            tip,
            state: 0,
            state_mask: 0,
            info: [0; 256],
            timeout_or_version: NotifyTimeout { version: 4 },
            info_title: [0; 64],
            info_flags: 0,
            guid_item: Guid {
                data1: 0,
                data2: 0,
                data3: 0,
                data4: [0; 8],
            },
            balloon_icon: null_mut(),
        }
    }

    fn native_thread(
        log_path: PathBuf,
        shared: Arc<NativeShared>,
        startup: mpsc::SyncSender<Result<(isize, String), ShellError>>,
    ) {
        let class_name = wide("AntiQuickPadShellMessageWindow");
        let instance = unsafe { GetModuleHandleW(null()) };
        let class = WndClassW {
            style: 0,
            wnd_proc: Some(wnd_proc),
            cls_extra: 0,
            wnd_extra: 0,
            instance,
            icon: null_mut(),
            cursor: null_mut(),
            background: null_mut(),
            menu_name: null(),
            class_name: class_name.as_ptr(),
        };
        unsafe {
            RegisterClassW(&class);
        }
        // HWND_MESSAGE is the documented pseudo-parent ((HWND)-3).
        let hwnd = unsafe {
            CreateWindowExW(
                0,
                class_name.as_ptr(),
                class_name.as_ptr(),
                0,
                0,
                0,
                0,
                0,
                (-3isize) as Hwnd,
                null_mut(),
                instance,
                null_mut(),
            )
        };
        if hwnd.is_null() {
            let error = message_error("CreateWindowExW(message-only)");
            log(&log_path, "SHELL_ERROR", &error.to_string());
            let _ = startup.send(Err(error));
            return;
        }
        let shared_ptr = Arc::into_raw(shared.clone());
        unsafe {
            SetWindowLongPtrW(hwnd, GWLP_USERDATA, shared_ptr as isize);
        }
        let icon = unsafe { LoadIconW(null_mut(), IDI_APPLICATION) };
        let mut data = tray_data(hwnd, "registering hotkey...");
        data.icon = icon;
        let tray_ok = unsafe { Shell_NotifyIconW(NIM_ADD, &data) != 0 };
        if !tray_ok {
            let error = message_error("Shell_NotifyIconW(NIM_ADD)");
            log(&log_path, "SHELL_ERROR", &error.to_string());
            unsafe {
                DestroyWindow(hwnd);
            }
            let _ = startup.send(Err(error));
            return;
        }
        let selected_hotkey = choose_hotkey(|chord| {
            let (modifiers, key) = match chord {
                HotkeyChord::Primary => (MOD_CONTROL | MOD_ALT | MOD_NOREPEAT, VK_SPACE),
                HotkeyChord::Fallback => (MOD_CONTROL | MOD_ALT | MOD_SHIFT | MOD_NOREPEAT, VK_A),
            };
            if unsafe { RegisterHotKey(hwnd, HOTKEY_ID, modifiers, key) } != 0 {
                Ok(())
            } else {
                let error = message_error(&format!("RegisterHotKey({})", chord.label()));
                if chord == HotkeyChord::Primary {
                    log(
                        &log_path,
                        "SHELL_ERROR",
                        &format!(
                            "primary hotkey registration failed; trying fallback {}: {error}",
                            HotkeyChord::Fallback.label()
                        ),
                    );
                }
                Err(error)
            }
        });
        let active_hotkey = match selected_hotkey {
            Ok(chord) => chord.label(),
            Err((primary_error, fallback_error)) => {
                let error = ShellError(format!(
                    "both hotkeys failed: {}: {primary_error}; {}: {fallback_error}",
                    HotkeyChord::Primary.label(),
                    HotkeyChord::Fallback.label()
                ));
                log(
                    &log_path,
                    "SHELL_ERROR",
                    &format!(
                        "hotkey registration failed; tray will be removed because no hotkey is active: {error}"
                    ),
                );
                unsafe {
                    Shell_NotifyIconW(NIM_DELETE, &data);
                    DestroyWindow(hwnd);
                }
                let _ = startup.send(Err(error));
                return;
            }
        };
        if let Ok(mut current) = shared.active_hotkey.lock() {
            *current = active_hotkey.to_owned();
        }
        data = tray_data(hwnd, active_hotkey);
        data.icon = icon;
        unsafe {
            Shell_NotifyIconW(NIM_MODIFY, &data);
        }
        log(
            &log_path,
            "REGISTER",
            &format!("tray_icon=true\thotkey={active_hotkey}\tresult=success"),
        );
        let _ = startup.send(Ok((hwnd as isize, active_hotkey.to_owned())));
        let mut msg: Msg = unsafe { zeroed() };
        loop {
            let result = unsafe { GetMessageW(&mut msg, null_mut(), 0, 0) };
            if result <= 0 {
                break;
            }
            unsafe {
                TranslateMessage(&msg);
                DispatchMessageW(&msg);
            }
        }
        unsafe {
            UnregisterHotKey(hwnd, HOTKEY_ID);
            Shell_NotifyIconW(NIM_DELETE, &data);
        }
        log(
            &log_path,
            "REGISTER",
            "tray_icon=false\thotkey_registered=false",
        );
    }
}

#[cfg(not(windows))]
mod platform {
    use super::*;
    pub struct Shell;
    impl Shell {
        pub fn start(_: PathBuf) -> Result<Self, ShellError> {
            Err(ShellError("the AntiQuickPad shell requires Windows".into()))
        }
        pub fn active_hotkey(&self) -> &str {
            ""
        }
        pub fn self_check(_: PathBuf) -> Result<String, ShellError> {
            Err(ShellError(
                "the AntiQuickPad shell self-check requires Windows".into(),
            ))
        }
        pub fn bind(&mut self, _: &str) -> Result<(), ShellError> {
            unreachable!()
        }
        pub fn poll(&mut self) -> Vec<ShellEvent> {
            Vec::new()
        }
        #[allow(dead_code)] // Available for app commands; hotkey/tray use the native dispatcher.
        pub fn toggle(&mut self) -> Result<(), ShellError> {
            unreachable!()
        }
        pub fn show(&mut self) -> Result<(), ShellError> {
            unreachable!()
        }
        pub fn hide(&mut self) -> Result<(), ShellError> {
            unreachable!()
        }
        pub fn dispose(&mut self) {}
    }
}

pub use platform::Shell;
