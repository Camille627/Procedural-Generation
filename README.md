# 🎯 Roguelike with Procedural Generation

🚀 A 2D shooter game where levels are **procedurally generated** using **graph theory**.

---

## 🌍 Procedural Generation - Principle

Our level generation system is based on **three steps**:

### **1️⃣ Graph Generation**
A **graph** is first created to structure the level. By using **graph theory**, we ensure that the player **does not have to go through the same corridor twice** to explore the entire map. To achieve this, the graph must be *Eulerian* and *planar* (otherwise, the edges cross).

🔹 Currently, our implementation generates a *linear* graph.

📌 **Next improvement:** Generate more innovative graphs: *cycles*, *semi-Eulerian* and *Eulerian* (non-linear) graphs.

### **2️⃣ Drawing Corridors and Rooms**
Each edge of the graph is replaced with a **corridor**, and each node is replaced with a **room**.
We have worked on making the corridors more natural and smooth, inspired by **additive synthesis**:

 🔹 We model a straight line between the **rooms** to be connected and then add a series of perturbations, with decreasing maximum amplitude at each iteration.

 🔹 The perturbations are sinusoidal curves whose frequency is divided by 2 at each iteration.

 🔹 Each perturbation is assigned a random amplitude modifier within the range [1;-1], which is a factor of the maximum amplitude of the perturbation. This introduces randomness into the layout.

📌 **Next improvement:** Generate **more complex rooms** (circular, rectangular, or more irregular shapes) to diversify environments.

### **3️⃣ Placing Entities in the Level**
Once the level structure is defined, we add **entities** inside the **rooms**:

📌 **Next improvement:** Place **entities and bonuses more realistically**, considering room layouts.

---

## 🧰 Game Mechanics
### Weapons
Weapons have five characteristics: fire rate, range, firing mode (auto or semi-auto), projectile speed, and damage.
The **Revolver** is the default weapon equipped by the player. The **Submachine Gun**, **Semi-Automatic Rifle**, and **Bolt-Action Rifle** can be found in **weapon crates**.

### Crates
When stepping on a **weapon crate**, you swap your weapon with the one inside. The **health crate** restores 1 **health point** if needed.
There is one of each type of crate per level.

### Victory 🏁
You complete a level when all enemies have been eliminated. A new level then loads.

---

## 🕹️ Try the Game Without Installing Unity

You can test the game without needing to install Unity!

### **1️⃣ Download the Compiled Version**
- The `build/` folder contains the compiled version of the game (`a3112b4`).

### **2️⃣ Launch the Game**
- Open the file **`My project.exe`**.  
- The game starts immediately, no installation required!

---

## 🎮 Game Controls Summary

| Action          | Default Key |
|----------------|------------|
| **Move**       | `W / A / S / D` |
| **Aim**        | `Left Shift` |
| **Shoot**      | `Left Click` |
| **Exit**       | `Escape` |
| **Activate a cheat code** | `\` |
| **Regenerate the level** | `G + E + N` |

To use a cheat code, hold the **Activate a cheat code** key along with the cheat code keys.
I was unable to regenerate the level during my last tests (both in the editor and in the compiled version).

---

## Known Issues

❌ **The level does not reload in the final build?**  
**Issue:** When the last enemy is killed, the level does not reload.

---

# Suggestions and Bug Reports
If you have improvement ideas or find a bug, feel free to share them with me 😉.

# License
This project is under the MIT license. You are free to use and modify it, but please credit the original author.

