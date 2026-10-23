// # spike: disposable notebook and carousel model; std-only by design.
use std::fs::{self, File};
use std::io::{self, Read, Write};
use std::path::{Path, PathBuf};

const MAGIC: &[u8; 8] = b"AQPNOTE1";
const WHEEL_BURST_IDLE_MS: u64 = 220;
const WHEEL_NOTCH: f64 = 1.0;
const RELEASE_DISTANCE: f64 = 0.35;
const RELEASE_VELOCITY: f64 = 1.0;
const SPRING_K: f64 = 180.0;
const SPRING_C: f64 = 26.0;

#[derive(Clone, Debug, PartialEq, Eq)]
pub struct Note {
    pub id: u64,
    pub text: String,
}

#[derive(Clone, Debug, PartialEq, Eq)]
pub struct Notebook {
    pub notes: Vec<Note>,
    pub current_index: usize,
    next_id: u64,
}

impl Default for Notebook {
    fn default() -> Self {
        Self {
            notes: vec![Note {
                id: 1,
                text: String::new(),
            }],
            current_index: 0,
            next_id: 2,
        }
    }
}

impl Notebook {
    pub fn new(notes: Vec<String>, current_index: usize) -> Self {
        let notes: Vec<Note> = notes
            .into_iter()
            .enumerate()
            .map(|(i, text)| Note {
                id: i as u64 + 1,
                text,
            })
            .collect();
        let mut this = Self {
            notes: if notes.is_empty() {
                vec![Note {
                    id: 1,
                    text: String::new(),
                }]
            } else {
                notes
            },
            current_index,
            next_id: 1,
        };
        this.next_id = this
            .notes
            .iter()
            .map(|n| n.id)
            .max()
            .unwrap_or(0)
            .saturating_add(1)
            .max(1);
        this.current_index = this.current_index.min(this.notes.len() - 1);
        this.prune_blank_interiors();
        this
    }

    pub fn add_note(&mut self, body: String) -> usize {
        while self.notes.iter().any(|note| note.id == self.next_id) {
            self.next_id = self.next_id.wrapping_add(1).max(1);
        }
        let id = self.next_id.max(1);
        self.next_id = id.wrapping_add(1).max(1);
        self.notes.push(Note { id, text: body });
        self.current_index = self.notes.len() - 1;
        self.current_index
    }

    /// Removes empty interior pages, while preserving the selected page and newest page.
    /// Returns the number of removed notes and remaps the selected index.
    pub fn prune_blank_interiors(&mut self) -> usize {
        if self.notes.is_empty() {
            self.notes.push(Note {
                id: 1,
                text: String::new(),
            });
            self.current_index = 0;
            return 0;
        }
        let old_current = self.current_index.min(self.notes.len() - 1);
        let newest = self.notes.len() - 1;
        let mut current = 0;
        let mut kept = Vec::with_capacity(self.notes.len());
        for (i, note) in self.notes.drain(..).enumerate() {
            let keep = i == old_current || i == newest || !note.text.trim().is_empty();
            if keep {
                if i == old_current {
                    current = kept.len();
                }
                kept.push(note);
            }
        }
        let removed = (newest + 1).saturating_sub(kept.len());
        self.notes = kept;
        if self.notes.is_empty() {
            self.notes.push(Note {
                id: 1,
                text: String::new(),
            });
            current = 0;
        }
        self.current_index = current.min(self.notes.len() - 1);
        removed
    }

    /// Length-prefixed binary format: magic, note count, selected index, then UTF-8 byte lengths/bodies.
    pub fn save_to(&self, path: impl AsRef<Path>) -> io::Result<()> {
        let path = path.as_ref();
        let temp = temp_path(path);
        let result = (|| {
            let mut file = File::create(&temp)?;
            file.write_all(MAGIC)?;
            write_u64(&mut file, self.notes.len() as u64)?;
            write_u64(
                &mut file,
                self.current_index.min(self.notes.len().saturating_sub(1)) as u64,
            )?;
            for note in &self.notes {
                write_u64(&mut file, note.id)?;
                write_u64(&mut file, note.text.len() as u64)?;
                file.write_all(note.text.as_bytes())?;
            }
            file.sync_all()?;
            replace_file(&temp, path)
        })();
        if result.is_err() {
            let _ = fs::remove_file(&temp);
        }
        result
    }

    pub fn restore_from(path: impl AsRef<Path>) -> io::Result<Self> {
        let mut file = File::open(path)?;
        let mut magic = [0; 8];
        file.read_exact(&mut magic)?;
        if &magic != MAGIC {
            return Err(invalid("bad notebook signature"));
        }
        let count = read_u64(&mut file)?;
        let selected = read_u64(&mut file)?;
        if count == 0 || count > 1_000_000 {
            return Err(invalid("invalid note count"));
        }
        if selected >= count {
            return Err(invalid("selected note is out of range"));
        }
        let mut notes = Vec::with_capacity(count as usize);
        let mut ids = std::collections::HashSet::with_capacity(count as usize);
        let mut total_bytes = 0_u64;
        for _ in 0..count {
            let id = read_u64(&mut file)?;
            if id == 0 || !ids.insert(id) {
                return Err(invalid("note ids must be unique and nonzero"));
            }
            let len = read_u64(&mut file)?;
            total_bytes = total_bytes
                .checked_add(len)
                .ok_or_else(|| invalid("notebook size overflow"))?;
            if len > 256 * 1024 * 1024 || total_bytes > 512 * 1024 * 1024 {
                return Err(invalid("notebook exceeds size limit"));
            }
            let mut bytes = vec![0; len as usize];
            file.read_exact(&mut bytes)?;
            notes.push(Note {
                id,
                text: String::from_utf8(bytes).map_err(|_| invalid("note is not UTF-8"))?,
            });
        }
        let mut trailing = [0; 1];
        if file.read(&mut trailing)? != 0 {
            return Err(invalid("trailing data"));
        }
        let mut notebook = Self {
            notes,
            current_index: selected as usize,
            next_id: 1,
        };
        notebook.next_id = notebook
            .notes
            .iter()
            .map(|n| n.id)
            .max()
            .unwrap_or(0)
            .saturating_add(1)
            .max(1);
        notebook.prune_blank_interiors();
        Ok(notebook)
    }
}

fn temp_path(path: &Path) -> PathBuf {
    let mut name = path.as_os_str().to_owned();
    name.push(format!(".{}.tmp", std::process::id()));
    PathBuf::from(name)
}

fn replace_file(temp: &Path, destination: &Path) -> io::Result<()> {
    #[cfg(windows)]
    {
        use std::os::windows::ffi::OsStrExt;
        #[link(name = "Kernel32")]
        unsafe extern "system" {
            fn MoveFileExW(existing: *const u16, replacement: *const u16, flags: u32) -> i32;
        }
        let from: Vec<u16> = temp.as_os_str().encode_wide().chain(Some(0)).collect();
        let to: Vec<u16> = destination
            .as_os_str()
            .encode_wide()
            .chain(Some(0))
            .collect();
        // MOVEFILE_REPLACE_EXISTING | MOVEFILE_WRITE_THROUGH. Failure leaves old save untouched.
        let ok = unsafe { MoveFileExW(from.as_ptr(), to.as_ptr(), 0x1 | 0x8) };
        if ok == 0 {
            return Err(io::Error::last_os_error());
        }
        Ok(())
    }
    #[cfg(not(windows))]
    {
        fs::rename(temp, destination)
    }
}
fn invalid(message: &'static str) -> io::Error {
    io::Error::new(io::ErrorKind::InvalidData, message)
}
fn write_u64(w: &mut impl Write, n: u64) -> io::Result<()> {
    w.write_all(&n.to_le_bytes())
}
fn read_u64(r: &mut impl Read) -> io::Result<u64> {
    let mut b = [0; 8];
    r.read_exact(&mut b)?;
    Ok(u64::from_le_bytes(b))
}

#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum CarouselEvent {
    None,
    PageChanged { index: usize },
    NoteCreated { index: usize },
}

/// `x` and `target` are page-width offsets from the currently selected note; positive means next.
/// Page selection commits immediately, while `x` animates from the old page to zero.
#[derive(Debug, Default)]
pub struct CarouselController {
    pub x: f32,
    pub target: f32,
    velocity: f64,
    last_tick_ms: Option<u64>,
    last_wheel_ms: Option<u64>,
    wheel_sum: f64,
    wheel_consumed: bool,
    drag: Option<Drag>,
}

#[derive(Clone, Copy, Debug)]
struct Drag {
    start_x: f64,
    last_x: f64,
    last_move_ms: u64,
    moved: bool,
}

impl CarouselController {
    /// Feed raw horizontal wheel `dx`. Negative `dx` advances to the next note, matching GPUI's
    /// platform event convention. Sub-notch deltas accumulate; the first full notch in a burst
    /// switches at most one page. A 220ms idle gap starts a fresh burst.
    pub fn wheel(&mut self, notebook: &mut Notebook, delta: f64, now_ms: u64) -> CarouselEvent {
        if !delta.is_finite() || delta == 0.0 {
            return CarouselEvent::None;
        }
        if self
            .last_wheel_ms
            .map_or(true, |t| now_ms.saturating_sub(t) > WHEEL_BURST_IDLE_MS)
        {
            self.wheel_sum = 0.0;
            self.wheel_consumed = false;
        }
        self.last_wheel_ms = Some(now_ms);
        if self.wheel_consumed {
            return CarouselEvent::None;
        }
        self.wheel_sum += delta;
        if self.wheel_sum.abs() < WHEEL_NOTCH {
            return CarouselEvent::None;
        }
        self.wheel_consumed = true;
        self.commit_direction(notebook, if self.wheel_sum < 0.0 { 1 } else { -1 })
    }

    pub fn begin_drag(&mut self) {
        self.drag = Some(Drag {
            start_x: self.x as f64,
            last_x: self.x as f64,
            last_move_ms: 0,
            moved: false,
        });
        self.velocity = 0.0;
    }

    /// `delta_pages` is total pointer displacement from drag start, in page widths.
    /// Pulling past the newest page is rubber-banded; other unavailable directions resist more.
    pub fn move_drag(
        &mut self,
        notebook: &Notebook,
        delta_pages: f64,
        velocity_pages_per_sec: f64,
        now_ms: u64,
    ) {
        if !delta_pages.is_finite() {
            return;
        }
        let Some(mut drag) = self.drag else {
            return;
        };
        let raw = drag.start_x + delta_pages;
        let index = notebook
            .current_index
            .min(notebook.notes.len().saturating_sub(1));
        let resisted =
            if (index == 0 && raw < 0.0) || (index + 1 < notebook.notes.len() && raw > 1.0) {
                if raw < 0.0 {
                    raw * 0.28
                } else {
                    1.0 + (raw - 1.0) * 0.28
                }
            } else if index + 1 == notebook.notes.len() && raw > 0.0 {
                raw * 0.48
            } else {
                raw
            };
        self.x = resisted as f32;
        self.target = self.x;
        self.velocity = if velocity_pages_per_sec.is_finite() {
            velocity_pages_per_sec
        } else {
            0.0
        };
        drag.last_x = resisted;
        drag.moved |= delta_pages.abs() > 0.0001;
        drag.last_move_ms = now_ms;
        self.drag = Some(drag);
    }

    pub fn end_drag(&mut self, notebook: &mut Notebook, now_ms: u64) -> CarouselEvent {
        let Some(drag) = self.drag.take() else {
            return CarouselEvent::None;
        };
        let direction = if self.x > 0.0 {
            1
        } else if self.x < 0.0 {
            -1
        } else {
            0
        };
        let distance = (self.x as f64).abs();
        let velocity = if drag.moved && now_ms.saturating_sub(drag.last_move_ms) <= 150 {
            self.velocity
        } else {
            0.0
        };
        let velocity_toward = velocity * direction as f64;
        let creating = direction > 0 && notebook.current_index + 1 == notebook.notes.len();
        let commit = drag.moved
            && direction != 0
            && (distance >= RELEASE_DISTANCE || (!creating && velocity_toward >= RELEASE_VELOCITY));
        if commit {
            // Convert visual displacement to offset relative to the newly selected index.
            let released_x = self.x;
            let event = self.commit_direction(notebook, direction);
            if matches!(
                event,
                CarouselEvent::PageChanged { .. } | CarouselEvent::NoteCreated { .. }
            ) {
                self.x = released_x - direction as f32;
                self.target = 0.0;
                self.last_tick_ms = None;
                return event;
            }
        }
        self.target = 0.0;
        CarouselEvent::None
    }

    /// Advance a damped spring with seconds derived from caller-provided monotonic milliseconds.
    pub fn tick(&mut self, now_ms: u64) {
        if self.drag.is_some() {
            return;
        }
        let Some(previous) = self.last_tick_ms.replace(now_ms) else {
            return;
        };
        let dt = now_ms.saturating_sub(previous) as f64 / 1000.0;
        if dt <= 0.0 {
            return;
        }
        let mut remaining = dt.min(0.1);
        while remaining > 0.0 {
            let step = remaining.min(1.0 / 120.0);
            let acceleration =
                SPRING_K * (self.target as f64 - self.x as f64) - SPRING_C * self.velocity;
            self.velocity += acceleration * step;
            self.x = (self.x as f64 + self.velocity * step) as f32;
            remaining -= step;
        }
        if (self.x - self.target).abs() < 0.001 && self.velocity.abs() < 0.01 {
            self.x = self.target;
            self.velocity = 0.0;
        }
    }

    fn commit_direction(&mut self, notebook: &mut Notebook, direction: i32) -> CarouselEvent {
        if direction > 0 {
            if notebook.current_index + 1 < notebook.notes.len() {
                notebook.current_index += 1;
                self.x = -1.0;
                self.target = 0.0;
                self.velocity = 0.0;
                self.last_tick_ms = None;
                CarouselEvent::PageChanged {
                    index: notebook.current_index,
                }
            } else {
                notebook.add_note(String::new());
                self.x = -1.0;
                self.target = 0.0;
                self.velocity = 0.0;
                self.last_tick_ms = None;
                CarouselEvent::NoteCreated {
                    index: notebook.current_index,
                }
            }
        } else if notebook.current_index > 0 {
            notebook.current_index -= 1;
            self.x = 1.0;
            self.target = 0.0;
            self.velocity = 0.0;
            self.last_tick_ms = None;
            CarouselEvent::PageChanged {
                index: notebook.current_index,
            }
        } else {
            self.target = 0.0;
            CarouselEvent::None
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::time::{SystemTime, UNIX_EPOCH};

    fn path(label: &str) -> PathBuf {
        let n = SystemTime::now()
            .duration_since(UNIX_EPOCH)
            .unwrap()
            .as_nanos();
        std::env::temp_dir().join(format!("aqp-{label}-{}-{n}.bin", std::process::id()))
    }

    fn raw(texts: &[&str], current_index: usize) -> Notebook {
        let notes = texts
            .iter()
            .enumerate()
            .map(|(i, text)| Note {
                id: i as u64 + 1,
                text: (*text).into(),
            })
            .collect();
        Notebook {
            notes,
            current_index,
            next_id: texts.len() as u64 + 1,
        }
    }

    #[test]
    fn default_notebook_has_one_editable_note() {
        assert_eq!(Notebook::default().notes[0].text, "");
    }
    #[test]
    fn constructor_clamps_current_index() {
        assert_eq!(Notebook::new(vec!["a".into()], 8).current_index, 0);
    }
    #[test]
    fn prune_preserves_selected_blank_and_newest() {
        let mut n = raw(&["a", "", " ", "b", ""], 2);
        assert_eq!(n.prune_blank_interiors(), 1);
        assert_eq!(
            n.notes.iter().map(|n| n.text.as_str()).collect::<Vec<_>>(),
            vec!["a", " ", "b", ""]
        );
        assert_eq!(n.current_index, 1);
    }
    #[test]
    fn prune_remaps_index_after_removed_blank() {
        let mut n = raw(&["a", "", "b", "c"], 2);
        assert_eq!(n.prune_blank_interiors(), 1);
        assert_eq!(n.current_index, 1);
        assert_eq!(n.notes[1].text, "b");
        assert_eq!(n.notes[1].id, 3);
    }
    #[test]
    fn save_restore_round_trips_utf8_and_current() {
        let p = path("roundtrip");
        let n = Notebook::new(vec!["héllo\n世界".into(), "next".into()], 1);
        n.save_to(&p).unwrap();
        assert_eq!(Notebook::restore_from(&p).unwrap(), n);
        let _ = fs::remove_file(p);
    }
    #[test]
    fn restore_rejects_truncated_and_corrupt_data() {
        let p = path("bad");
        fs::write(&p, b"AQPNOTE1\x01").unwrap();
        assert!(Notebook::restore_from(&p).is_err());
        fs::write(&p, b"not a notebook").unwrap();
        assert!(Notebook::restore_from(&p).is_err());
        let _ = fs::remove_file(p);
    }
    #[test]
    fn wheel_subnotches_coalesce_to_one_page() {
        let mut n = Notebook::new(vec!["a".into(), "b".into(), "c".into()], 0);
        let mut c = CarouselController::default();
        assert_eq!(c.wheel(&mut n, -0.4, 0), CarouselEvent::None);
        assert_eq!(
            c.wheel(&mut n, -0.6, 50),
            CarouselEvent::PageChanged { index: 1 }
        );
        assert_eq!(c.wheel(&mut n, -2.0, 90), CarouselEvent::None);
        assert_eq!(n.current_index, 1);
    }
    #[test]
    fn wheel_idle_starts_new_burst() {
        let mut n = Notebook::new(vec!["a".into(), "b".into()], 0);
        let mut c = CarouselController::default();
        assert_eq!(
            c.wheel(&mut n, -1.0, 0),
            CarouselEvent::PageChanged { index: 1 }
        );
        assert_eq!(
            c.wheel(&mut n, 1.0, 221),
            CarouselEvent::PageChanged { index: 0 }
        );
    }
    #[test]
    fn wheel_past_newest_creates_only_one_note_per_burst() {
        let mut n = Notebook::default();
        let mut c = CarouselController::default();
        assert_eq!(
            c.wheel(&mut n, -1.0, 0),
            CarouselEvent::NoteCreated { index: 1 }
        );
        assert_eq!(c.wheel(&mut n, -1.0, 100), CarouselEvent::None);
        assert_eq!(n.notes.len(), 2);
    }
    #[test]
    fn drag_follows_pointer_then_commits_and_springs_from_new_index() {
        let mut n = Notebook::new(vec!["a".into(), "b".into()], 0);
        let mut c = CarouselController::default();
        c.begin_drag();
        c.move_drag(&n, 0.6, 0.0, 20);
        assert!((c.x - 0.6).abs() < 1e-6);
        assert_eq!(
            c.end_drag(&mut n, 25),
            CarouselEvent::PageChanged { index: 1 }
        );
        assert!((c.x + 0.4).abs() < 1e-6);
        assert_eq!(c.target, 0.0);
    }
    #[test]
    fn drag_velocity_can_commit_short_distance() {
        let mut n = Notebook::new(vec!["a".into(), "b".into()], 0);
        let mut c = CarouselController::default();
        c.begin_drag();
        c.move_drag(&n, 0.1, 1.4, 20);
        assert_eq!(
            c.end_drag(&mut n, 25),
            CarouselEvent::PageChanged { index: 1 }
        );
    }
    #[test]
    fn newest_edge_drag_rubber_bands_then_creates_page() {
        let mut n = Notebook::default();
        let mut c = CarouselController::default();
        c.begin_drag();
        c.move_drag(&n, 0.1, 3.0, 5);
        assert_eq!(c.end_drag(&mut n, 10), CarouselEvent::None);
        assert_eq!(
            n.notes.len(),
            1,
            "creation requires overscroll distance, even for fast flicks"
        );
        c = CarouselController::default();
        c.begin_drag();
        c.move_drag(&n, 1.2, 0.0, 20);
        assert!(c.x > 0.0 && c.x < 1.2);
        assert_eq!(
            c.end_drag(&mut n, 25),
            CarouselEvent::NoteCreated { index: 1 }
        );
    }
    #[test]
    fn rejected_drag_springs_home() {
        let mut n = Notebook::new(vec!["a".into(), "b".into()], 0);
        let mut c = CarouselController::default();
        c.begin_drag();
        c.move_drag(&n, 0.1, 0.0, 20);
        assert_eq!(c.end_drag(&mut n, 25), CarouselEvent::None);
        assert_eq!(c.target, 0.0);
    }
    #[test]
    fn click_and_stale_velocity_do_not_switch() {
        let mut n = Notebook::new(vec!["a".into(), "b".into()], 0);
        let mut c = CarouselController::default();
        c.begin_drag();
        c.move_drag(&n, 0.0, 0.0, 20);
        assert_eq!(c.end_drag(&mut n, 25), CarouselEvent::None);
        c.begin_drag();
        c.move_drag(&n, 0.1, 2.0, 30);
        assert_eq!(c.end_drag(&mut n, 300), CarouselEvent::None);
        assert_eq!(n.current_index, 0);
    }
    #[test]
    fn spring_tick_moves_toward_target_and_settles() {
        let mut c = CarouselController::default();
        c.x = 1.0;
        c.target = 0.0;
        c.tick(0);
        c.tick(16);
        assert!(c.x < 1.0);
        for t in (32..5000).step_by(16) {
            c.tick(t);
        }
        assert_eq!(c.x, 0.0);
    }
}
