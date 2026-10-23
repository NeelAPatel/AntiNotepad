// # spike — native registration/dispatch self-check without opening the GPUI app.
#[path="mod.rs"] mod shell;
fn main() {
    let path=std::env::args_os().nth(1).map(std::path::PathBuf::from).expect("supply self-check log path");
    match shell::Shell::self_check(path) { Ok(result)=>println!("{result}"),Err(error)=>{eprintln!("{error}");std::process::exit(1)} }
}
