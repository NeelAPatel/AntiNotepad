// # spike — disposable full capability spike; runtime validation remains separate.
mod editor;
mod model;
mod perf;
mod shell;

use gpui_kit::component::ActiveTheme;
use gpui_kit::component::input::{EditorState, InputEvent};
use gpui_kit::*;
use model::{CarouselController, CarouselEvent, Notebook};
use std::{
    cell::RefCell,
    collections::HashMap,
    fs::OpenOptions,
    io::Write,
    path::PathBuf,
    rc::Rc,
    time::{Duration, Instant, SystemTime, UNIX_EPOCH},
};

fn log_line(path: &PathBuf, kind: &str, detail: &str) {
    let micros = SystemTime::now()
        .duration_since(UNIX_EPOCH)
        .unwrap_or_default()
        .as_micros();
    if let Ok(mut file) = OpenOptions::new().create(true).append(true).open(path) {
        let _ = writeln!(file, "{micros}\t{kind}\t{detail}");
    }
}

fn horizontal_steps(delta: ScrollDelta) -> f32 {
    match delta {
        ScrollDelta::Pixels(delta) => f32::from(delta.x) / 40.0, // optional, untested smooth-input scaling
        ScrollDelta::Lines(delta) => {
            #[link(name = "user32")]
            unsafe extern "system" {
                fn SystemParametersInfoW(
                    action: u32,
                    param: u32,
                    value: *mut u32,
                    flags: u32,
                ) -> i32;
            }
            let mut chars = 0u32;
            if unsafe { SystemParametersInfoW(108, 0, &mut chars, 0) } != 0
                && chars > 0
                && chars != u32::MAX
            {
                delta.x / chars as f32
            } else {
                delta.x
            }
        }
    }
}

struct Scratchpad {
    book: Notebook,
    editors: HashMap<u64, Entity<EditorState>>,
    subscriptions: Vec<Subscription>,
    carousel: CarouselController,
    shell: Option<shell::Shell>,
    data_path: PathBuf,
    log: PathBuf,
    start: Instant,
    last_save: Instant,
    dirty: bool,
    allow_save: bool,
    status: String,
    material: usize,
    drag: Option<(f32, f32, u64)>,
    width: f32,
    first_frame: bool,
    animation_pending: bool,
    last_edit: Instant,
    stats: Rc<RefCell<perf::FrameStats>>,
}
impl Scratchpad {
    fn new(log: PathBuf, start: Instant, window: &mut Window, cx: &mut Context<Self>) -> Self {
        window.set_window_title("AntiQuickPad B");
        log_line(
            &log,
            "TEXT_SETTINGS",
            &format!(
                "ui_font={}\tui_px={}\tmono_font={}\tmono_px={}\tmono_line_height=1.5em\tscale={}\tmode={:?}\teffective_antialias=grayscale_nonopaque\thinting=DirectWrite_recommended_grid_fit_natural",
                cx.theme().font_family,
                f32::from(cx.theme().font_size),
                cx.theme().mono_font_family,
                f32::from(cx.theme().mono_font_size),
                window.scale_factor(),
                cx.text_rendering_mode()
            ),
        );
        let data_dir = PathBuf::from(env!("CARGO_MANIFEST_DIR")).join("data");
        let data_path = data_dir.join("notes.bin");
        let _ = std::fs::create_dir_all(&data_dir);
        let (book, allow_save, status) = if data_path.exists() {
            match Notebook::restore_from(&data_path) {
                Ok(book) => (book, true, "Restored saved notes".into()),
                Err(error) => (
                    Notebook::new(vec![editor::FIXTURE.into()], 0),
                    false,
                    format!("Restore failed; autosave disabled to preserve file: {error}"),
                ),
            }
        } else {
            (Notebook::new(vec![editor::FIXTURE.into(), "# Second note\n\nWheel changes one page per burst.\n\nDrag the blue page handle; ordinary editor drags select text.\n".into(), "# Newest note\n\nPull the page handle left past the threshold, or wheel forward, to create a blank note.\n".into()], 0), true, "Fixture loaded; autosave local".into())
        };
        let native_shell = match shell::Shell::start(log.clone()) {
            Ok(mut native) => match native.bind("AntiQuickPad B") {
                Ok(()) => {
                    let _ = native.show();
                    Some(native)
                }
                Err(error) => {
                    log_line(&log, "SHELL_ERROR", &error.to_string());
                    None
                }
            },
            Err(error) => {
                log_line(&log, "SHELL_ERROR", &error.to_string());
                None
            }
        };
        let mut this = Self {
            book,
            editors: HashMap::new(),
            subscriptions: Vec::new(),
            carousel: CarouselController::default(),
            shell: native_shell,
            data_path,
            log,
            start,
            last_save: Instant::now(),
            dirty: true,
            allow_save,
            status,
            material: 0,
            drag: None,
            width: 900.0,
            first_frame: false,
            animation_pending: false,
            last_edit: Instant::now(),
            stats: Rc::new(RefCell::new(perf::FrameStats::new())),
        };
        this.ensure_editors(window, cx);
        this.focus_editor(window, cx);
        let weak = cx.entity().downgrade();
        window.on_window_should_close(cx, move |_, cx| {
            let mut should_close = false;
            let _ = weak.update(cx, |this, cx| {
                this.sync_text(cx);
                this.save();
                if let Some(native) = &mut this.shell {
                    let _ = native.hide();
                } else {
                    should_close = true;
                }
            });
            should_close
        });
        cx.spawn_in(window, async move |weak, cx| {
            loop {
                cx.background_executor()
                    .timer(Duration::from_millis(50))
                    .await;
                if weak
                    .update_in(cx, |this, window, cx| this.tick(window, cx))
                    .is_err()
                {
                    break;
                }
            }
        })
        .detach();
        this
    }
    fn now(&self) -> u64 {
        self.start.elapsed().as_millis() as u64
    }
    fn ensure_editors(&mut self, window: &mut Window, cx: &mut Context<Self>) {
        for note in &self.book.notes {
            if !self.editors.contains_key(&note.id) {
                let state = editor::create(note.text.clone(), window, cx);
                let subscription =
                    cx.subscribe_in(&state, window, |this, _, event: &InputEvent, _, cx| {
                        if matches!(event, InputEvent::Change) {
                            this.dirty = true;
                            this.last_edit = Instant::now();
                            cx.notify();
                        }
                    });
                self.subscriptions.push(subscription);
                self.editors.insert(note.id, state);
            }
        }
        self.editors
            .retain(|id, _| self.book.notes.iter().any(|note| note.id == *id));
    }
    fn sync_text(&mut self, cx: &App) {
        for note in &mut self.book.notes {
            if let Some(state) = self.editors.get(&note.id) {
                note.text = state.read(cx).value().to_string();
            }
        }
    }
    fn focus_editor(&self, window: &mut Window, cx: &mut Context<Self>) {
        if let Some(note) = self.book.notes.get(self.book.current_index) {
            if let Some(state) = self.editors.get(&note.id) {
                state.update(cx, |state, cx| state.focus(window, cx));
            }
        }
    }
    fn save(&mut self) {
        if !self.allow_save || !self.dirty {
            return;
        }
        match self.book.save_to(&self.data_path) {
            Ok(()) => {
                self.status = format!("Saved {} notes locally", self.book.notes.len());
                self.dirty = false;
                log_line(
                    &self.log,
                    "AUTOSAVE",
                    &format!(
                        "notes={}\tindex={}",
                        self.book.notes.len(),
                        self.book.current_index
                    ),
                );
            }
            Err(error) => {
                self.status = format!("Autosave failed: {error}");
                log_line(&self.log, "SAVE_ERROR", &error.to_string());
            }
        }
        self.last_save = Instant::now();
    }
    fn after_page(&mut self, event: CarouselEvent, window: &mut Window, cx: &mut Context<Self>) {
        if event != CarouselEvent::None {
            self.sync_text(cx);
            self.book.prune_blank_interiors();
            self.ensure_editors(window, cx);
            self.focus_editor(window, cx);
            self.dirty = true;
            self.last_edit = Instant::now();
            log_line(
                &self.log,
                "PAGE",
                &format!(
                    "event={event:?}\tnotes={}\tindex={}",
                    self.book.notes.len(),
                    self.book.current_index
                ),
            );
        }
        if event != CarouselEvent::None {
            cx.notify();
        }
    }
    fn wheel(&mut self, dx: f32, window: &mut Window, cx: &mut Context<Self>) {
        self.stats.borrow_mut().wheel_events += 1;
        let now = self.now();
        let event = self.carousel.wheel(&mut self.book, dx as f64, now);
        if event == CarouselEvent::None {
            self.stats.borrow_mut().coalesced_wheel += 1;
        }
        self.after_page(event, window, cx);
    }
    fn tick(&mut self, window: &mut Window, cx: &mut Context<Self>) {
        let events = self
            .shell
            .as_mut()
            .map(|native| native.poll())
            .unwrap_or_default();
        for event in events {
            match event {
                shell::ShellEvent::Shown => {
                    cx.activate(true);
                    self.focus_editor(window, cx);
                    log_line(&self.log, "EDITOR_REFOCUS", "show_event");
                    let log = self.log.clone();
                    let state = self
                        .editors
                        .get(&self.book.notes[self.book.current_index].id)
                        .unwrap()
                        .clone();
                    window.on_next_frame(move |window, cx| {
                        log_line(
                            &log,
                            "SHOW_EDITABLE_FRAME",
                            &format!(
                                "editor_focus={}",
                                state.read(cx).focus_handle(cx).is_focused(window)
                            ),
                        );
                    });
                }
                shell::ShellEvent::Hidden => {
                    self.sync_text(cx);
                    self.save();
                    self.drag = None;
                }
                shell::ShellEvent::Quit => {
                    self.sync_text(cx);
                    self.save();
                    cx.quit();
                    return;
                }
                shell::ShellEvent::Error(error) => {
                    self.status = error;
                    cx.notify();
                }
            }
        }
        let moving = self.drag.is_some() || self.carousel.x.abs() > 0.0001;
        self.stats.borrow_mut().state(moving);
        let sample = self.stats.borrow_mut().sample();
        if let Some(sample) = sample {
            log_line(&self.log, "FRAME_STATS", &sample);
        }
        if self.dirty && !moving && self.last_edit.elapsed() > Duration::from_millis(500) {
            self.sync_text(cx);
            self.save();
            cx.notify();
        }
    }
    fn material(&mut self, mode: usize, window: &mut Window, cx: &mut Context<Self>) {
        self.material = mode;
        if let Some(native) = &mut self.shell {
            native.clear_backdrop();
        }
        window.set_background_appearance(WindowBackgroundAppearance::Opaque);
        window.set_background_appearance(match mode {
            0 => WindowBackgroundAppearance::Transparent,
            1 => WindowBackgroundAppearance::MicaBackdrop,
            _ => WindowBackgroundAppearance::Blurred,
        });
        log_line(
            &self.log,
            "MATERIAL_REQUEST",
            &["Transparent", "MicaBackdrop", "Blurred"][mode],
        );
        cx.notify();
    }
}
impl Render for Scratchpad {
    fn render(&mut self, window: &mut Window, cx: &mut Context<Self>) -> impl IntoElement {
        self.width = (f32::from(window.viewport_size().width) - 32.0).max(320.0);
        let width = self.width;
        self.stats.borrow_mut().renders += 1;
        self.stats
            .borrow_mut()
            .state(self.drag.is_some() || self.carousel.x.abs() > 0.0001);
        if !self.first_frame {
            self.first_frame = true;
            let started = self.start;
            let log = self.log.clone();
            let state = self
                .editors
                .get(&self.book.notes[self.book.current_index].id)
                .unwrap()
                .clone();
            window.on_next_frame(move |window, cx| {
                log_line(
                    &log,
                    "FIRST_EDITABLE_FRAME",
                    &format!(
                        "main_to_frame_ms={:.3}\teditor_focus={}\tpid={}",
                        started.elapsed().as_secs_f64() * 1000.0,
                        state.read(cx).focus_handle(cx).is_focused(window),
                        std::process::id()
                    ),
                );
            });
        }
        if self.drag.is_none() && self.carousel.x.abs() > 0.0001 && !self.animation_pending {
            self.animation_pending = true;
            let animated = cx.entity().downgrade();
            window.on_next_frame(move |_, cx| {
                let _ = animated.update(cx, |this, cx| {
                    this.animation_pending = false;
                    this.carousel.tick(this.now());
                    cx.notify();
                });
            });
        }
        let weak = cx.entity().downgrade();
        let move_weak = weak.clone();
        let up_weak = weak.clone();
        let paint_stats = self.stats.clone();
        let observer = canvas(
            |_, _, _| (),
            move |_, _, window, _| {
                paint_stats.borrow_mut().paint();
                window.on_mouse_event(move |event: &ScrollWheelEvent, phase, window, cx| {
                    if phase != DispatchPhase::Capture {
                        return;
                    }
                    let dx = horizontal_steps(event.delta);
                    if dx.abs() > 0.001 {
                        let _ = weak.update(cx, |this, cx| this.wheel(dx, window, cx));
                        cx.stop_propagation();
                    }
                });
                window.on_mouse_event(move |event: &MouseMoveEvent, phase, _, cx| {
                    if phase != DispatchPhase::Capture {
                        return;
                    }
                    let _ = move_weak.update(cx, |this, cx| {
                        if let Some((start_x, last_x, last_ms)) = this.drag {
                            this.stats.borrow_mut().drag_events += 1;
                            if event.pressed_button != Some(MouseButton::Left) {
                                this.drag = None;
                                this.carousel = CarouselController::default();
                                log_line(&this.log, "DRAG_CANCEL", "missing_pressed_button");
                                cx.notify();
                                return;
                            }
                            let now = this.now();
                            let x = f32::from(event.position.x);
                            if (x - last_x).abs() < 0.01 {
                                return;
                            }
                            let velocity = -(x - last_x) as f64
                                / this.width as f64
                                / ((now.saturating_sub(last_ms).max(1)) as f64 / 1000.0);
                            this.carousel.move_drag(
                                &this.book,
                                -(x - start_x) as f64 / this.width as f64,
                                velocity,
                                now,
                            );
                            this.drag = Some((start_x, x, now));
                            cx.notify();
                        }
                    });
                });
                window.on_mouse_event(move |event: &MouseUpEvent, phase, window, cx| {
                    if phase != DispatchPhase::Capture || event.button != MouseButton::Left {
                        return;
                    }
                    let _ = up_weak.update(cx, |this, cx| {
                        if this.drag.take().is_some() {
                            let now = this.now();
                            let event = this.carousel.end_drag(&mut this.book, now);
                            this.after_page(event, window, cx);
                        }
                    });
                });
            },
        )
        .absolute()
        .size_full();
        let index = self.book.current_index;
        let x = self.carousel.x;
        let pages = self
            .book
            .notes
            .iter()
            .enumerate()
            .filter(|(i, _)| {
                *i == index || (x > 0.0001 && *i == index + 1) || (x < -0.0001 && *i + 1 == index)
            })
            .map(|(i, note)| {
                let state = self.editors.get(&note.id).unwrap();
                div()
                    .absolute()
                    .left(px((i as f32 - index as f32 - x) * width))
                    .top(px(0.0))
                    .w(px(width))
                    .h_full()
                    .p_3()
                    .child(editor::view(state))
                    .into_any_element()
            })
            .collect::<Vec<_>>();
        let save_status = if self.dirty {
            format!("{} · unsaved changes", self.status)
        } else {
            self.status.clone()
        };
        let shell_status = self
            .shell
            .as_ref()
            .map(|native| format!("{} · tray right-click to quit", native.active_hotkey()))
            .unwrap_or_else(|| "SHELL ERROR: tray/hotkey unavailable (see runtime log)".into());
        div().relative().size_full().flex().flex_col().p_4().gap_2()
            .bg(if self.material==0 { rgba(0x101722b0) } else { rgba(0x10172270) }).text_color(rgb(0xe6edf3))
            .child(observer)
            .child(div().flex().gap_3().items_center()
                .child(div().child(format!("AntiQuickPad B · note {}/{}",index+1,self.book.notes.len())).on_mouse_down(MouseButton::Left,|_,window,_|window.start_window_move()))
                .child(div().id("transparent").p_2().bg(rgba(0x5099ff44)).child("Transparent").on_click(cx.listener(|this,_,window,cx| this.material(0,window,cx))))
                .child(div().id("mica").p_2().bg(rgba(0x5099ff44)).child("Mica").on_click(cx.listener(|this,_,window,cx| this.material(1,window,cx))))
                .child(div().id("blur").p_2().bg(rgba(0x5099ff44)).child("Blur").on_click(cx.listener(|this,_,window,cx| this.material(2,window,cx))))
                .child(div().id("hide").p_2().child("Hide").on_click(cx.listener(|this,_,_,_| { if let Some(native)=&mut this.shell { let _=native.hide(); } })))
                .child(div().id("quit").p_2().child("Quit").on_click(cx.listener(|this,_,_,cx| { this.sync_text(cx); this.save(); cx.quit(); }))))
            .child(shell_status)
            .child(div().id("page-drag-handle").h(px(40.0)).bg(rgba(0x3184d866)).p_2()
                .child("⇆ DRAG PAGE HERE · wheel anywhere switches pages · editor drags select text")
                .on_mouse_down(MouseButton::Left,cx.listener(|this,event:&MouseDownEvent,_,cx| {
                    this.sync_text(cx);
                    this.carousel.begin_drag();
                    let x=f32::from(event.position.x);
                    this.drag=Some((x,x,this.now()));
                    cx.stop_propagation(); cx.notify();
                })))
            .child(div().flex_1().min_h_0().relative().overflow_hidden().children(pages))
            .child(div().text_size(px(12.0)).child(save_status))
    }
}
fn main() {
    let start = Instant::now();
    let log = std::env::var_os("ANTIQUICKPAD_PROBE_LOG")
        .map(PathBuf::from)
        .unwrap_or_else(|| {
            PathBuf::from(env!("CARGO_MANIFEST_DIR")).join("../../docs/spikes/B/runtime.tsv")
        });
    log_line(
        &log,
        "SESSION",
        &format!(
            "full_spike_start\tpid={}\tkit=0.7.3\tgpui=0.3.9",
            std::process::id()
        ),
    );
    if std::env::args().any(|arg| arg == "--shell-self-check") {
        match shell::Shell::self_check(log) {
            Ok(message) => println!("{message}"),
            Err(error) => {
                eprintln!("{error}");
                std::process::exit(1);
            }
        }
        return;
    }
    gpui_kit::application().run(move |cx| {
        gpui_kit::init(cx);
        gpui_kit::open_window(
            WindowOptions {
                window_bounds: Some(WindowBounds::Windowed(Bounds::centered(
                    None,
                    size(px(1060.0), px(760.0)),
                    cx,
                ))),
                titlebar: None,
                window_decorations: Some(WindowDecorations::Client),
                window_background: WindowBackgroundAppearance::Transparent,
                ..Default::default()
            },
            cx,
            |window, cx| cx.new(|cx| Scratchpad::new(log.clone(), start, window, cx)),
        )
        .expect("open scratchpad");
        cx.activate(true);
    });
}
