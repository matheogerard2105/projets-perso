# Projets personnels

**Mathéo GERARD** – Élève ingénieur en 5e année à Polytech Lille, spécialité
**Informatique, Statistique et Intelligence Artificielle (ISIA)**.

Ce dépôt rassemble les applications et les jeux que je développe en dehors des cours. Ils
correspondent à la rubrique « Projets personnels » de mon CV Développeur. Mes projets d'école
sont dans le dépôt [projets-ecole](https://github.com/matheogerard2105/projets-ecole).

---

## Organisation du dépôt

```
projets-perso/
├── paddock26/           → simulateur de course automobile 3D (navigateur + Android)
├── turn-it/             → jeu en ligne : grille qui pivote, IA, parties classées (web + Android)
├── turn-it-unity/       → première version de Turn It, sous Unity (C#)
├── quadrille/           → casse-tête logiques et défis entre amis (web, Android, iOS)
├── nofon/               → application Android de gestion du temps d'écran
└── releve-terrain-qr/   → outil métier : inventaire de chantier par QR code
```

### Pourquoi ce classement ?

Les projets sont classés **par type** puis **du plus abouti au plus récent** :

1. **Jeux** (Paddock 26, Turn It, Quadrille) : ce sont mes projets les plus complets. Ils couvrent
   un moteur de simulation, une IA de jeu, du multijoueur en ligne avec un serveur, une base de
   données et la publication sur mobile. Turn It est présent en deux versions : la version web
   actuelle et la première version développée sous Unity en C#.
2. **Applications** (Nofon, Relevé Terrain QR) : des applications qui répondent à un besoin
   concret. Nofon est une application Android native en Kotlin avec un backend Supabase.
   Relevé Terrain QR est un outil que j'ai créé pour le travail de mon père, qu'il utilise avec des clients.

Chaque dossier contient son propre README qui détaille le projet, son architecture et la façon
de le lancer.

---

## Jeux

| Projet | Description | Technologies |
|---|---|---|
| [Paddock 26](paddock26) | Simulateur de course automobile en 3D inspiré du règlement 2026 : moteur physique indépendant du rendu (adhérence, usure des pneus, météo, stratégies des IA), 24 circuits, mode carrière, multijoueur en salon, replays et export vidéo pour les réseaux sociaux. | JavaScript, three.js, Web Audio, Node.js, Android (WebView) |
| [Turn It](turn-it) | Jeu d'alignement de pions sur une grille qui pivote : IA minimax sur 13 niveaux, multijoueur en ligne validé par le serveur, parties classées avec système elo, application installable (PWA) et Android. | JavaScript, Node.js, WebSocket, SQLite |
| [Turn It – version Unity](turn-it-unity) | Première version mobile de Turn It, développée sous Unity. La logique du jeu se trouve dans `Assets/GameManager.cs`. | Unity, C# |
| [Quadrille](quadrille) | Entraînement illimité aux casse-tête logiques Zip, Queens, Tango et Patches, avec des grilles générées automatiquement (solution unique, trois niveaux de difficulté) et des défis entre amis par code, avec classement en direct. | JavaScript, Capacitor (Android, iOS), Supabase |

## Applications

| Projet | Description | Technologies |
|---|---|---|
| [Nofon](nofon) | Application Android pour reprendre le contrôle de son temps d'écran : objectifs avec mises (argent simulé), seul ou en groupe, badges, fil d'activité entre amis. Toute la logique sensible est côté serveur. | Kotlin, Jetpack Compose, MVVM, Hilt, Room, Supabase (PostgreSQL, Row Level Security) |
| [Relevé Terrain QR](releve-terrain-qr) | Application mobile créée pour le travail de mon père, technicien : inventaire des équipements d'un chantier par scan de QR code, suivi de l'avancement par étage et par local, import et export Excel. Fonctionne hors connexion. | JavaScript, IndexedDB, jsQR, SheetJS |

---

## Essayer les applications

- **Quadrille** (version web) : https://matheogerard2105.github.io/projets-perso/quadrille/app/www/
- **Relevé Terrain QR** : https://matheogerard2105.github.io/projets-perso/releve-terrain-qr/
- **Quadrille pour Android** : fichier APK dans les
  [Releases](https://github.com/matheogerard2105/projets-perso/releases) de ce dépôt.
