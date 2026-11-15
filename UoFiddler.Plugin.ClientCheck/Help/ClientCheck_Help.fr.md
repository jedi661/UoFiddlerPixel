# Vérification du client – Aide

La vérification du client compare **tous les fichiers du client classique entre eux** et montre ce qui ne concorde pas. Ce sont des erreurs que l'on ne remarque sinon qu'en jeu : objets invisibles, taches noires sur la carte, noms manquants, créatures aux mouvements incorrects.

Les fichiers actuellement chargés dans UoFiddler sont vérifiés (dossier du client défini dans les options, y compris vos modifications en mémoire).

## Utilisation

1. Choisir sous **Contrôles** ce qui doit être vérifié (par défaut : tout). Le choix est mémorisé.
2. Cliquer sur **▶ Lancer la vérification**. La progression s'affiche dans la barre d'état. UoFiddler reste utilisable ; **■ Annuler** arrête la vérification.
3. Les résultats apparaissent dans la liste. Un résumé en haut indique les erreurs, avertissements, infos et le nombre par domaine.
4. Cliquer sur une entrée : à droite s'affichent la **description**, **Comment corriger** et un **aperçu** (objet, tuile de terrain, gump ou animation).
5. Corriger les entrées corrigeables avec **Corriger** (plusieurs entrées sélectionnées possibles) ou toutes les entrées affichées avec **Tout corriger…**.
6. **Exporter le rapport** enregistre les entrées affichées en page HTML, tableau CSV ou texte.

La langue se change en haut à droite (Deutsch, English, Français). Elle s'applique immédiatement à tous les textes, y compris les résultats et cette aide.

## Niveaux

| Niveau | Signification |
|---|---|
| **Erreur** | Il manque sûrement quelque chose en jeu (invisible, cassé). À corriger. |
| **Avertissement** | Fonctionne, mais s'affiche ou se comporte mal (ex. noir sur la carte). |
| **Info** | Incomplet, mais généralement sans gravité (ex. graphisme sans nom). |

## Filtres et utilisation

- **Afficher :** filtrer par niveau et par domaine.
- **Recherche :** cherche dans l'ID et la description (ex. `0x1F03`, `Body 400`, `Felucca`).
- **Masquer les corrigés :** masque les entrées déjà corrigées (marquées ✓).
- Cliquer sur un **en-tête de colonne** pour trier ; un second clic inverse l'ordre.
- **Ctrl+C** ou **Copier la ligne** copie les entrées sélectionnées, **Copier l'ID** seulement l'ID.

## Les contrôles en détail

### Art ↔ tiledata
- **Graphisme sans nom** (info) : l'objet a un graphisme mais pas de nom dans tiledata. Solution : onglet « TileData », définir nom et flags, enregistrer tiledata.mul.
- **tiledata sans graphisme** (info) : une entrée tiledata a un nom mais pas de graphisme. Solution : ajouter le graphisme dans l'onglet « Items » ou vider l'entrée.

### Tuiles de terrain
- **Tuile sans nom** (info).
- **Texture manquante** (avertissement) : tiledata renvoie à une texture inexistante ; les pentes s'affichent mal. Solution : ajouter la texture dans l'onglet « Textures » ou mettre TextureId à 0.

### Équipement
Tous les objets avec le flag **Wearable** sont vérifiés.
- **Pas de couche** (avertissement) : `Quality` vaut 0, le serveur ne sait pas où l'objet est porté.
- **Animation manquante** (erreur) : le champ tiledata `Animation` renvoie à une animation inexistante – l'objet est invisible une fois porté.
- **Gump de paperdoll manquant** (avertissement) : le gump **50000 + animation** (homme) manque et aucun remplacement n'existe dans gump.def – l'objet n'apparaît pas sur le paperdoll.
- **Gump féminin manquant** (info) : le gump **60000 + animation** manque ; le client utilise celui de l'homme (souvent voulu).
- Le sac à dos (couche 21) et les montures (couche 25) n'ont pas de gump propre et sont ignorés.

### Corps / animations
Les corps 1 à 4095 sont vérifiés.
- **Pas d'entrée dans mobtypes.txt** (avertissement) : une animation existe, mais le client ne connaît pas le type (MONSTER, SEA_MONSTER, ANIMAL, HUMAN, EQUIPMENT) et joue de mauvaises actions. **Corrigeable :** la ligne proposée est écrite dans une copie de mobtypes.txt dans le **dossier de sortie** (sauvegarde `.bak`). Copier ensuite le fichier dans le client.
- **mobtypes.txt sans animation** (info) : ligne sans animation correspondante, généralement un reste sans gravité.
- **Bodyconv.def ne pointe vers rien** (erreur) : un corps est redirigé vers anim2–anim5, mais il n'y a pas de données – la créature est invisible.
- **body.def ne pointe vers rien** (avertissement) : un corps est redirigé vers un corps sans animation.

### radarcol (couleurs de carte)
radarcol.mul contient **une couleur** pour chaque tuile de terrain et chaque objet, pour le radar et la carte. Sans elle, l'objet est **noir** sur la carte.
- **Pas de couleur radar** (avertissement) : **corrigeable.** « Corriger » attribue la couleur moyenne du graphisme (en mémoire). Cliquer ensuite sur **Enregistrer radarcol.mul** – le fichier est écrit dans le dossier de sortie et doit être copié dans le client. Le bouton est orange tant que des modifications ne sont pas enregistrées.
- **radarcol.mul trop court** (avertissement) : le fichier n'a pas de place pour les ID d'objets étendus et doit être agrandi.
- Les tuiles contenant « NoDraw » dans leur nom sont ignorées (invisibles volontairement).

### Multis
- **Élément sans graphisme** (erreur) : une maison ou un bateau contient des ID d'objets sans graphisme ; ces parties manquent en jeu. Solution : ajouter le graphisme ou remplacer l'élément dans l'éditeur de multis.

### Statiques sur les cartes
Toutes les cartes sont lues (staidx/statics). Les cartes utilisant les mêmes fichiers (ex. Felucca et Trammel) ne sont lues qu'une fois.
- **L'objet n'existe pas** (erreur) : des statiques avec un ID d'objet sans graphisme sont placées sur la carte. Une entrée par ID avec le nombre et une position d'exemple (`carte X,Y,Z`).
- **Teinte invalide** (avertissement) : statiques avec un numéro de teinte supérieur au nombre de teintes de hues.mul.
- **Erreur de lecture** (erreur) : staidx et statics ne vont pas ensemble ou sont endommagés.

### Cliloc (noms)
Le client affiche les noms d'objets via cliloc : **1020000 + ID**, à partir de l'ID 0x4000 **1078872 + ID** (comme ServUO `Item.LabelNumber`).
- **Pas de nom cliloc** (info) : il n'y a pas d'entrée pour l'objet dans Cliloc.enu. Solution : ajouter l'entrée dans l'onglet « Cliloc ».

## Corrections automatiques – ce qui se passe exactement

| Quoi | Où c'est écrit |
|---|---|
| Couleur radar | en mémoire, puis **Enregistrer radarcol.mul** → dossier de sortie |
| Ligne mobtypes.txt | copie `mobtypes.txt` dans le dossier de sortie (copiée depuis le client la première fois), sauvegarde `.bak` |

Rien n'est jamais écrit **directement dans le dossier du client**. Copiez vous-même les fichiers modifiés dans le client lorsque tout est correct.

## Conseils

- Après une correction, relancez simplement la vérification : les problèmes corrigés disparaissent.
- Pour un contrôle rapide après l'ajout de nouveaux objets, les contrôles **Art ↔ tiledata**, **Équipement**, **radarcol** et **Cliloc** suffisent.
- La vérification des cartes prend quelques secondes par grande carte.
- Le rapport HTML est pratique à transmettre à l'équipe : il est classé par domaine et indique la solution pour chaque entrée.

## Fichiers de cette extension

- `ClientCheckPlugin.dll` dans le dossier `plugins` d'UoFiddler
- Aide : `plugins\ClientCheckHelp\ClientCheck_Help.de.md`, `…en.md`, `…fr.md`
- Paramètres (langue, contrôles choisis) : `ClientCheck.lang` et `ClientCheck.checks` dans le dossier AppData d'UoFiddler
