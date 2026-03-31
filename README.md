# 🎬 StudioCollections — Plugin Jellyfin

Affiche une **section horizontale par studio** sur la page d'accueil de Jellyfin :
- Logo du studio (image `Logo` de Jellyfin)
- Au survol : backdrop du studio + miniatures des titres récents
- Clic → filtre la bibliothèque par studio
- Entièrement configurable depuis le dashboard admin

---

## 📸 Fonctionnalités

| Fonctionnalité | Détail |
|---|---|
| Section studios scrollable | Défilement horizontal, cards 200×120 px |
| Logo studio | Image `Logo` fournie par Jellyfin/TMDB |
| Hover backdrop | Fond animé (backdrop du studio) au survol |
| Hover mini-preview | Vignettes des 5 derniers titres du studio |
| Configuration admin | Titre, nombre de studios, seuil minimum, media types |
| Injection automatique | Script JS injecté dans `index.html` au démarrage |
| API REST | `/StudioCollections/studios` pour récupérer les données |

---

## ⚙️ Prérequis

- **Jellyfin** ≥ 10.9.0
- **.NET SDK** 8.0
- Les logos/backdrops de studios doivent être téléchargés (via les métadonnées Jellyfin — TMDB les fournit souvent automatiquement)

---

## 🔨 Compilation

```bash
git clone <your-repo>
cd Jellyfin.Plugin.StudioCollections
dotnet build -c Release
```

Le fichier compilé se trouve dans :
```
Jellyfin.Plugin.StudioCollections/bin/Release/net8.0/Jellyfin.Plugin.StudioCollections.dll
```

---

## 📦 Installation manuelle

1. Compiler le plugin (cf. ci-dessus)
2. Créer le dossier plugin dans votre répertoire Jellyfin :
   ```bash
   # Linux (chemin par défaut)
   mkdir -p ~/.local/share/jellyfin/plugins/StudioCollections/

   # Docker — adapter selon votre volume
   mkdir -p /path/to/jellyfin/plugins/StudioCollections/
   ```
3. Copier le DLL :
   ```bash
   cp Jellyfin.Plugin.StudioCollections/bin/Release/net8.0/Jellyfin.Plugin.StudioCollections.dll \
      ~/.local/share/jellyfin/plugins/StudioCollections/
   ```
4. Redémarrer Jellyfin
5. Vider le cache navigateur (`Ctrl+F5`)

---

## 🗂️ Structure du projet

```
Jellyfin.Plugin.StudioCollections/
├── Plugin.cs                          # Classe principale du plugin
├── PluginServiceRegistrator.cs        # Enregistrement DI
├── ScriptInjector.cs                  # Injection du script JS dans index.html
├── Configuration/
│   ├── PluginConfiguration.cs         # Modèle de configuration
│   ├── PluginConfigurationPage.cs     # Enregistrement de la page admin
│   └── configPage.html                # Page de configuration (ressource embarquée)
├── Controllers/
│   ├── StudioCollectionsController.cs # API REST studios
│   └── ClientScriptController.cs      # Serveur le script JS
└── Web/
    └── studioCollections.js           # Script front-end (ressource embarquée)
```

---

## 🌐 API REST

### `GET /StudioCollections/studios`
Retourne la liste des studios avec leurs métadonnées.

**Réponse :**
```json
[
  {
    "name": "Marvel Studios",
    "logoUrl": "/Items/abc123/Images/Logo",
    "backdropUrl": "/Items/abc123/Images/Backdrop",
    "itemCount": 42,
    "browseUrl": "/web/#/list.html?studios=Marvel+Studios",
    "recentItems": [
      {
        "id": "def456",
        "name": "Avengers: Endgame",
        "primaryImageUrl": "/Items/def456/Images/Primary",
        "type": "Movie",
        "productionYear": 2019
      }
    ]
  }
]
```

### `GET /StudioCollections/studios/{studioName}/items`
Retourne les items d'un studio spécifique.

### `GET /StudioCollections/config`
Retourne la configuration publique du plugin.

### `GET /StudioCollections/clientscript`
Sert le fichier JavaScript front-end.

---

## ⚙️ Configuration (Dashboard Admin)

Allez dans **Dashboard → Plugins → StudioCollections**.

| Option | Défaut | Description |
|---|---|---|
| Titre de la section | `Par Studio` | Texte affiché au-dessus de la rangée |
| Nombre max de studios | `20` | Limiter les studios affichés |
| Minimum d'items | `3` | Studios avec moins de X titres ignorés |
| Types de médias | `Movies,Series` | Filtrer par type |
| Studios avec logo uniquement | `false` | Cacher les studios sans logo |
| Effet hover | `true` | Activer backdrop + miniatures au survol |
| Durée animation hover | `400` ms | Vitesse de la transition |

---

## 💡 Astuces

**Les logos n'apparaissent pas ?**
- Allez dans **Dashboard → Médiathèques → Scanner les métadonnées**
- Activez l'option "Télécharger les images des studios" dans vos fournisseurs de métadonnées (TMDB)

**La section n'apparaît pas ?**
- Vérifiez que Jellyfin a accès en écriture à `index.html`
- Vérifiez les logs Jellyfin pour `[StudioCollections]`
- Videz le cache navigateur

---

## 📄 Licence

GPL-3.0 — Inspiré de [jellyfin-plugin-template](https://github.com/jellyfin/jellyfin-plugin-template) et [Jellyfin-MonWUI-Plugin](https://github.com/G-grbz/Jellyfin-MonWUI-Plugin).
