# Projet ORIF

Ce projet regroupe plusieurs composants et fonctionnalités, organisés en différentes parties pour le développement d'applications VR, de rapports académiques et d'un créateur de scénarios.

## Structure du projet

### 1. Répertoire `ORIF`
Ce répertoire contient les fichiers nécessaires pour le développement et la publication de l'application VR.

#### Sous-répertoires et fichiers clés :
- `Assets` : Contient les ressources Unity comme les scripts, modèles 3D et textures.

### 2. Répertoire `APK`
Ce répertoire contient la ou les APK de l'application prête(s) à être installée(s).

#### Fichiers clé(s) :
- `ORIF_DATE.APK` : APK datée de l'application.

### 3. Répertoire `Website`
Ce répertoire contient un créateur de scénarios interactif basé sur Node.js.

#### Sous-répertoires et fichiers clés :
- `scenario_creator` : Code source principal pour le créateur de scénarios.
- `commands.txt` : Liste des commandes ou instructions pour exécuter le projet.
- `index.html` : Page d'accueil du site web.
- `server.js` : Script serveur pour gérer les scénarios.
- `scenarios.db` : Base de données pour stocker les scénarios créés.

### 4. Répertoire `Documentation`
Ce répertoire contient les divers éléments liés au lancement de l'application.

#### Sous-répertoires et fichiers clés :
- `scenario_creator` : Informations sur l'application de création de scénarios.
- `orif` : Informations sur l'application ORIF et sa brève prise en main.

## Instructions de génération de l'APK et du serveur de scénario

### Application VR
1. Ouvrir le projet Unity dans le répertoire `ORIF`.
2. Compiler et exécuter le projet pour générer une nouvelle version de l'APK.
3. Installer le fichier APK sur un appareil Meta Quest 3 pour tester.

### Créateur de scénarios
1. Accéder au répertoire `Website/scenario_creator`.
2. Installer les dépendances avec :
   ```bash
   npm install
   ```
3. Démarrer le serveur avec :
   ```bash
   node server.js
   ```
   Ou avec :
   ```bash
   npm start
   ```

4. Accéder à l'application dans un navigateur via `http://localhost:5000` ou si le projet est lancé sur un serveur `http://[adresse du serveur]:5000`.

## Technologies utilisées

- **Unity** en **C#** avec le **XR Interaction Toolkit** ainsi que le **XR Hands**: Pour le développement de l'application VR.
- **Node.js** : Pour le backend du créateur de scénarios.
- **HTML/CSS/JS** : Pour l'interface utilisateur web.
- **Base de données SQLite** : Pour la gestion des scénarios.

## Auteur
Ce projet a été réalisé par Ackermann Gawen dans le cadre de l'initiative du projet ORIF, il a pour auteur HEPIA.
