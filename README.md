# 🚗 Road Rush

- Inspired by the charm of `Crossy Road`, this arcade Unity runner is about daring hops between roads and railways.  
- Keep the pace, dodge cars and trains, and push your high-score distance!

---

## 🎮 Controls

| Action | Keys |
|--------|------|
| Move | WASD or Arrow keys |
| Jump | Space |
| Pause / Menu | Esc |
| Change skin | Numbers 1–8 |

---

## 🧠 Architecture

- **LineGeneratorManager** — orchestrates the sequence of islands, roads, rivers, and rails  
- **VehicleSpawner** / **VehicleMover** — handle spawning and movement of road traffic  
- **TrainSpawner** — runs train cycles with warnings and signaling  
- **RiverSpawner** / **FloatMover** — build river sections with moving platforms  
- **Player** — controls jumping, input, skin switching, and obstacle interactions  
- **DistanceCounter** — tracks travelled distance and updates UI  
- **GameSoundManager** — It is a sound manager that adds sounds of cars, trains and more

---

## 📸 Screenshots
<img width="2559" height="1439" alt="Screenshot_3" src="https://github.com/user-attachments/assets/9dcb1bc5-c0e0-4bec-af3f-284e6c2b2b42" />
<img width="2559" height="1439" alt="Screenshot_2" src="https://github.com/user-attachments/assets/00d305aa-20af-47d3-a982-781961cc00fc" />
<img width="2559" height="1439" alt="Screenshot_1" src="https://github.com/user-attachments/assets/3c24b396-8342-4602-9084-67f6535f1a93" />


---

## 📦 Links

- 🎮 [Play on itch.io](https://aerunstudio.itch.io/road-rush)  

---

## 📜 License

MIT License — free to use with credit.

