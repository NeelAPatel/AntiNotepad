// # spike — observed root redraws, not GPU utilization or compositor presents.
use std::time::{Duration, Instant};

pub struct FrameStats {
    window: Instant,
    accounted: Instant,
    moving: bool,
    idle_ms: f64,
    moving_ms: f64,
    pub renders: u64,
    pub paints: u64,
    idle_paints: u64,
    moving_paints: u64,
    last_moving_paint: Option<Instant>,
    max_moving_gap_ms: f64,
    pub wheel_events: u64,
    pub coalesced_wheel: u64,
    pub drag_events: u64,
}
impl FrameStats {
    pub fn new() -> Self {
        let now = Instant::now();
        Self {
            window: now,
            accounted: now,
            moving: false,
            idle_ms: 0.0,
            moving_ms: 0.0,
            renders: 0,
            paints: 0,
            idle_paints: 0,
            moving_paints: 0,
            last_moving_paint: None,
            max_moving_gap_ms: 0.0,
            wheel_events: 0,
            coalesced_wheel: 0,
            drag_events: 0,
        }
    }
    pub fn state(&mut self, moving: bool) {
        let now = Instant::now();
        let ms = now.duration_since(self.accounted).as_secs_f64() * 1000.0;
        if self.moving {
            self.moving_ms += ms;
        } else {
            self.idle_ms += ms;
        }
        if !moving {
            self.last_moving_paint = None;
        }
        self.moving = moving;
        self.accounted = now;
    }
    pub fn paint(&mut self) {
        self.paints += 1;
        if self.moving {
            self.moving_paints += 1;
            let now = Instant::now();
            if let Some(last) = self.last_moving_paint {
                self.max_moving_gap_ms = self
                    .max_moving_gap_ms
                    .max(now.duration_since(last).as_secs_f64() * 1000.0);
            }
            self.last_moving_paint = Some(now);
        } else {
            self.idle_paints += 1;
        }
    }
    pub fn sample(&mut self) -> Option<String> {
        if self.window.elapsed() < Duration::from_secs(1) {
            return None;
        }
        self.state(self.moving);
        let seconds = self.window.elapsed().as_secs_f64();
        let line = format!(
            "seconds={seconds:.3}\troot_renders={}\troot_paints={}\troot_paints_per_sec={:.2}\tidle_ms={:.1}\tidle_paints={}\tanimating_ms={:.1}\tanimating_paints={}\tmax_animating_paint_gap_ms={:.2}\twheel_events={}\tcoalesced_wheel={}\tdrag_events={}",
            self.renders,
            self.paints,
            self.paints as f64 / seconds,
            self.idle_ms,
            self.idle_paints,
            self.moving_ms,
            self.moving_paints,
            self.max_moving_gap_ms,
            self.wheel_events,
            self.coalesced_wheel,
            self.drag_events
        );
        let phase = self.moving;
        *self = Self::new();
        self.moving = phase;
        Some(line)
    }
}
