# Éditeur de régions

L'éditeur de régions permet de dessiner des régions (villes, donjons, zones gardées, zones sans maisons …) directement sur les cartes de votre client et de les écrire pour **ServUO/RunUO**, **Sphere** ou **POL**. Les cartes proviennent de la même liste que l'onglet carte (y compris vos cartes ajoutées via « Gérer les cartes »).

## Présentation

- **En haut :** menu Fichier, annuler/rétablir, choix de la carte, vérifier, paramètres, aide, langue, apparence (clair/sombre).
- **Deuxième barre :** outils, grille, zoom, affichage, client.
- **À gauche :** arbre de toutes les régions de la carte choisie (sous-régions en retrait), recherche (Ctrl+F).
- **Au centre :** la carte avec les régions. En bas à droite la mini-carte – un clic y saute.
- **À droite :** onglets **Propriétés**, **Zones**, **Vérification**, **Historique**.
- **En bas :** position, tuile de terrain et hauteur sous la souris, régions sous la souris, messages.

## Outils

| Outil | Touche | Fonction |
|---|---|---|
| Sélection | V | cliquer une région ; glisser un rectangle = déplacer ; poignées = redimensionner ; Maj = déplacer toute la région ; Alt = région suivante en dessous |
| Déplacer | H | faire glisser la carte (le bouton du milieu fonctionne toujours) |
| Rectangle | R | tracer une zone = nouvelle région |
| Polygone | P | cliquer les coins, double-clic/Entrée = terminer, Retour arrière = retirer un point, Échap = annuler |
| Cercle | C | glisser depuis le centre |
| Remplir | F | terrain contigu du même type dans la partie visible (île, forêt, désert, lac …) |
| Go | G | placer le point « go » de la région choisie |
| Entrée | E | placer l'entrée de la région choisie (DungeonRegion) |

En dessinant :

- **sans touche** = nouvelle région (si la région choisie n'a pas encore de zone, elle reçoit la zone)
- **Maj** = ajouter la zone à la région choisie
- **Alt** = découper la zone de la région choisie

La **grille** (1, 2, 4, 8, 16) aligne rectangles et points – 8 correspond aux blocs de la carte.

Molette = zoom, flèches = déplacer la carte, Suppr = supprimer la zone ou la région choisie.

## Modifier les régions

- **＋ Région / ＋ Sous-région** (Inser / Maj+Inser). Puis dessiner la zone.
- **Renommer :** F2 dans l'arbre ou champ « Nom ».
- **Imbriquer :** glisser une région de l'arbre sur une autre = elle devient sous-région. Maj en relâchant = l'insérer avant. Relâcher sur un espace vide = premier niveau.
- **Menu contextuel de l'arbre :** dupliquer, afficher sur la carte, envoyer le client au point « go », copier en XML ServUO, optimiser les rectangles, convertir les polygones, masquer, verrouiller.
- **Menu contextuel de la carte :** choisir la région ici, placer go/entrée ici, supprimer la zone, envoyer le client ici, copier la position.
- Les régions **verrouillées** ne peuvent pas être déplacées sur la carte. Les régions **masquées** ne sont pas dessinées (et, au choix, pas exportées).

### Propriétés

| Champ | Signification |
|---|---|
| Nom | nom de la région (ServUO : `name`) |
| Type | classe C# du serveur : TownRegion, DungeonRegion, GuardedRegion, NoHousingRegion, Jail … – types personnalisés autorisés |
| Priorité | la plus haute l'emporte en cas de chevauchement (50 par défaut) ; les sous-régions en héritent |
| Musique | `MusicName` de ServUO (POL : numéro midi, converti) |
| Plage Z | `<zrange>` de toute la région ; par rectangle dans l'onglet Zones |
| Go | destination de [go et des runes ; ⌖ = choisir sur la carte |
| Entrée | `<entrance>` d'une DungeonRegion, éventuellement sur une autre carte |
| Gardes | `<guards disabled>` et type de garde personnalisé |
| Rune | `<rune name>` |
| Options | `smartNoHousing`, `logoutDelay` (grisé = non écrit), masquée, verrouillée |
| Couleur | couleur propre dans l'éditeur, sinon selon le type |
| Notes | uniquement dans le projet |
| Clés serveur | autres valeurs pour Sphere (`EVENTS`, `FLAGS`, `GROUP`, `TAG.…`) et POL (`POL.EnterText`, `POL.NoCombat` …) |

### Zones

Liste de tous les rectangles (1, 2, …) et polygones (P1, P2 …). Saisir les valeurs en bas puis **Ajouter** ou **Appliquer**. **Optimiser** fusionne les rectangles en un minimum. Les serveurs ne connaissent que des rectangles – polygones et cercles sont convertis automatiquement à l'export.

## Annuler / historique

Chaque modification (y compris le glisser sur la carte, les imports, les corrections automatiques) peut être annulée avec **Ctrl+Z** et rétablie avec **Ctrl+Y**. L'onglet **Historique** liste toutes les étapes ; « Revenir jusqu'ici » annule plusieurs étapes d'un coup.

## Vérification

L'onglet **Vérification** trouve notamment : régions sans nom ou sans zone, noms en double, rectangles hors carte ou sans taille, valeurs Z incorrectes, sous-régions hors de leur région, chevauchements de même priorité, point « go » hors de la région ou dans l'eau, entrée dans l'eau, musique inconnue, types de région personnalisés. Les problèmes marqués 🔧 se corrigent automatiquement. Un double-clic montre la région sur la carte. La vérification s'exécute automatiquement avant l'écriture dans un serveur.

## Fichiers et formats

| Format | Lire | Écrire |
|---|---|---|
| Projet `.uoregions` (JSON) | ✔ | ✔ – tout, y compris polygones, couleurs, notes |
| ServUO / RunUO `Data\Regions.xml` | ✔ | ✔ – les éléments inconnus (p. ex. `<spawning>`) et les commentaires sont conservés |
| Sphere `mapN_areas.scp` (`[AREADEF]`, `[ROOMDEF]`) | ✔ | ✔ |
| POL `regions.cfg`, `justice.cfg`, `music.cfg` | ✔ | ✔ |
| CSV | ✔ | ✔ |
| Texte, aperçu HTML, image PNG de la carte | – | ✔ |
| Classes C# des types de région personnalisés (ServUO) | – | ✔ |

## Définir les dossiers du serveur

Dans **⚙ Paramètres** :

- **Dossier ServUO / RunUO** – le dossier principal du serveur (ou `Data`, ou directement `Regions.xml`).
- **Fichier des zones Sphere** – p. ex. `scripts\maps\map0\map0_areas.scp`. Si le fichier s'appelle `mapN…`, seules les régions de cette carte sont écrites.
- **Dossier regions de POL** – le dossier contenant `regions.cfg` (et `justice.cfg`, `music.cfg` s'ils sont utilisés).
- **Dossier d'export** – cible de « Tout dans le dossier d'export ». Vide : `<AppData>\RegionExport`.

Les paramètres sont enregistrés dans `<AppData>\UoFiddler\RegionEditor.json` dès le clic sur **OK** et rechargés au prochain démarrage. Sous les chemins, on voit si le fichier a été trouvé et combien de régions il contient. **Charger** à côté d'un chemin enregistre les paramètres et charge aussitôt les régions de ce serveur : elles apparaissent sur la carte et dans l'arbre, et la première région s'affiche avec ses propriétés et zones. Avec **Charger au démarrage**, l'éditeur ouvre automatiquement le dernier projet ou les régions de ServUO, Sphere ou POL.

Le menu **⇅ Serveur** de la barre supérieure indique les serveurs définis et charge ou enregistre en un clic.

Ensuite :

- **Fichier → Charger depuis le serveur** lit les régions directement dans le serveur configuré.
- **Fichier → Enregistrer dans le serveur** les réécrit. Une sauvegarde `fichier.AAAAMMJJ-HHMMSS.bak` est toujours créée avant. Redémarrez ensuite le serveur.
- **Regions.xml existant :** *remplacer entièrement*, *remplacer seulement les facettes du projet* (les autres restent) ou *remplacer/ajouter les régions par nom* (tout le reste reste inchangé).
- **Fichier → Exporter → Tout dans le dossier d'export** crée :
  - `servuo\Data\Regions.xml` (+ `servuo\Scripts\Regions\*.cs` pour les types personnalisés)
  - `sphere\mapN\mapN_areas.scp` par carte
  - `pol\regions\regions.cfg`, `justice.cfg`, `music.cfg`
  - `regions.csv`, `regions.txt`, `regions.html`, `regions.uoregions`

  Copiez ensuite ces fichiers dans votre serveur ServUO, Sphere ou POL.

## Remarques sur les formats

- **ServUO :** `<rect>` est écrit en `x/y/width/height` ou `x1/y1/x2/y2` (paramètre). x2/y2 est la fin (non incluse). Les sous-régions n'ont leur propre priorité/musique que si elle diffère de la région parente.
- **Sphere :** `RECT=x1,y1,x2,y2,map` – x2/y2 est écrit comme fin, comme pour ServUO. Les sous-régions deviennent des `AREADEF` séparés avec `GROUP=` de la région parente. Les FLAGS sont complétés selon le type (Town/Guarded → `REGION_FLAG_GUARDED`, Dungeon → `REGION_FLAG_UNDERGROUND`, les trois → `REGION_FLAG_NOBUILDING`).
- **POL :** `Range x1 y1 x2 y2` avec coins inclus. Les régions à plusieurs rectangles reçoivent plusieurs lignes `Range` dans un même bloc. Les realms sont britannia, britannia_alt, ilshenar, malas, tokuno, termur.
- Les facettes absentes de la liste des cartes restent dans le projet et sont aussi exportées.

## Client

- **⌖ Client** saute à la position du client lancé, **⟳ Suivre** la suit.
- « Envoyer le client ici » utilise la même commande que l'onglet carte (options : commande et arguments de carte, p. ex. `.goforce`).
