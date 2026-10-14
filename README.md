# MyFitnessStats
---

MyFitnessStas is a mobile app developed on .NET MAUI 9.0. It is a fitness tracking application to track your running activities.

<img width="426" height="888" alt="image" src="https://github.com/user-attachments/assets/0c140406-45a3-4f46-9c7d-2f1e7cbe097e" />


---

The app allows you:
#### 1. To start a run
#### 2. Track live session info 
#### 3. View location/map info
#### 4. Receive hardware feedback
#### 5. Customise some accessibility settings
#### 6. View a history of your runs

---

The purpose of MyFitnessStats is to provide a clean and accesible fitness experience.

## Main Features

- Run tracking with timer, distance, and speed
- Live GPS map route tracking
- Compass view with heading and direction
- Run summary after stopping a session
- Run history page
- Light, dark, and system theme support
- Adjustable text size for accessibility
- Auto-pause using accelerometer movement detection
- Manual pause/resume
- Text-to-speech and vibration feedback
- Help page with user instructions

  
## Hardware Features

The application uses multiple mobile hardware features:

### 1. Geolocation / GPS

Used to retrieve the user's current location and support map-based tracking.

### 2. Accelerometer

Used for movement detection and auto-pause behaviour. If no movement is detected, the app can automatically pause the run. Movement resumes the run when auto-pause is enabled.

### 3. Haptic Feedback / Vibration

Used to provide physical feedback when the user starts, pauses, resumes, or stops a run.

### 4. Compass

### 5. Text-to-speech
---

The app follows the MVVM pattern:

- `Views` — XAML pages
- `ViewModels` — app logic and commands
- `Models` — data classes
- `Services` — settings, accessibility, and history logic

The app demonstrates UI/UX design, accessibility features, mobile hardware integration, functionality and error handling.
