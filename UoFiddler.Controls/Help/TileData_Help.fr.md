# TileData – aide

L'onglet TileData modifie les propriétés des **objets** (statiques) et des **tuiles de terrain** du fichier `tiledata.mul`, ainsi que les **fichiers texte de ClassicUO**.

Toutes les modifications se font d'abord **en mémoire**. Elles ne sont écrites dans le fichier qu'avec **Save Tiledata** (`tiledata.mul` dans le dossier de sortie).

## Présentation

| Onglet | Contenu |
|---|---|
| Items | liste des objets, image, valeurs (nom, poids, hauteur …) et flags |
| Land Tiles | liste des tuiles de terrain, image, nom, id de texture et flags |
| ClassicUO | éditeur de `chair.txt`, `cave.txt`, `tree.txt` … de ClassicUO |

Les entrées rouges diffèrent de l'état du fichier. Après **Save Tiledata**, une annulation ou une restauration, elles redeviennent noires.

## Sélection multiple

Dans les deux listes (objets et terrain) :

- **Clic** – choisir une tuile
- **Ctrl + clic** – ajouter la tuile à la sélection ou l'en retirer
- **Maj + clic** – plage depuis la dernière tuile cliquée (dans un même groupe)
- **Clic droit → Select ID range…** – choisir par ids, p. ex. `0x0B2C-0x0B40, 0x0C00, 3000..3010`
- **Clic droit → Clear selection** – seule la tuile affichée reste choisie

Les tuiles choisies ont un fond bleu clair. Les champs montrent toujours les valeurs de la dernière tuile cliquée.

### La même modification sur plusieurs tuiles

1. Choisir les tuiles (Ctrl/Maj).
2. Dans les champs, modifier **seulement ce qui doit valoir pour toutes**, p. ex. Height = 5 et la coche *Surface*.
3. Cliquer **Save changes**.

**Avec une seule tuile**, tous les champs sont écrits.

**Avec plusieurs tuiles**, seuls les champs et flags modifiés depuis le clic sont écrits. Les autres valeurs des tuiles (nom, animation …) restent inchangées. Cela vaut aussi pour les modèles, *Insert settings* et le bouton d'effacement : ils remplissent les champs, et **Save changes** applique les modifications à toutes les tuiles choisies.

Avec **Save directly on changes** (menu), chaque modification est appliquée aussitôt à **toutes les tuiles choisies**.

## Modification groupée (Batch edit…)

Pour les gros travaux, **Batch edit…** (barre ou clic droit) ouvre une fenêtre dédiée.

**Quelles tuiles :**
- les tuiles choisies
- une plage d'ids (`0x0B2C-0x0B40, 3000..3010`)
- toutes les tuiles de la liste (avec le filtre de l'onglet)

On peut aussi limiter à « noms contenant … » et « seulement avec le flag … ».

**Valeurs :** seuls les champs cochés sont modifiés. Saisir une valeur coche le champ.
- Nombres : *définir*, *ajouter* ou *soustraire*. La valeur reste dans sa plage valide (0–255 ou ±32767).
- Nom : *définir*, *préfixe*, *suffixe* ou *remplacer* (20 caractères max.).

**Flags :** *set* ajoute le flag, *remove* le retire. La colonne *have it* indique combien de tuiles l'ont déjà.

En bas s'affiche le nombre de tuiles réellement modifiées. **Apply** fait tout en **une seule** étape d'annulation.

## Annuler / rétablir

- **↶ Undo** / **Ctrl+Z** et **↷ Redo** / **Ctrl+Y** (ou Ctrl+Maj+Z)
- L'annulation fonctionne pour tout : champs, flags, Save changes, Batch edit, import CSV, modèles, Insert settings, Set textures, restauration.
- Plusieurs frappes rapprochées dans le même champ comptent pour une seule étape.
- Après le rechargement des fichiers, l'historique recommence.
- Ctrl+Y dans la liste des objets reste « Push marked » tant que le bouton de mémorisation est actif.

## Modifications et restauration

**Changes…** liste tous les objets et tuiles de terrain qui diffèrent du fichier, avec le détail (p. ex. `Weight 1 → 255; +Wall -Surface`).

- **Show** (ou double-clic) va à la tuile.
- **Revert selected** / **Revert all** remet les tuiles à l'état du fichier. Cela peut aussi être annulé.
- **Copy list** copie la liste en texte.

**Clic droit → Revert to file state** restaure directement les tuiles choisies.

## Fichiers ClassicUO

ClassicUO lit des informations supplémentaires dans `<ClassicUO>\Data\Client\*.txt`.

Configuration : choisir le dossier de ClassicUO avec **Load**. Le dossier `Data` ou `Data\Client` convient aussi, le bon sous-dossier est trouvé. Puis choisir le fichier dans la liste.

| Fichier | Contenu | Format |
|---|---|---|
| chair.txt | sièges | `GRAPHIC,D1,D2,D3,D4,OFFSET_Y,MIRROR_OFFSET_Y` (direction 0/2/4/6, -1 = impossible) |
| cave.txt | tuiles de grotte | un id graphique par ligne (décimal) |
| vegetation.txt | herbe, buissons … (masquables) | un id graphique par ligne (décimal) |
| tree.txt | arbres → souches | `GRAPHIC=FLAG` (0/1) |
| containers.txt | gumps de conteneurs | `GUMP OPEN_SOUND CLOSE_SOUND X Y W H [ICON] [MIN_X] [MIN_Y]` |
| seasons.txt | saisons | `SEASON,static/landtile,ORIGINAL,REMPLACEMENT` (virgules seulement, sans espaces) |
| lights.txt | lumière des objets | `ITEM_ID SHADER` ou `ITEM_ID H<hue>` |
| lightshaders.txt | couleurs de lumière | `ID RGB R_CURVE [G_CURVE B_CURVE]` (courbes Standard, A–E) |

Les lignes de commentaire commencent par `#` ou `;` (seasons.txt : `#` ou `//`). Les ids sont décimaux ; seuls seasons.txt et lights.txt acceptent aussi `0x…`.

**Fonctions de l'éditeur :**
- La description du fichier choisi s'affiche à droite.
- Sous les boutons s'affiche à quoi se rapporte la ligne sous le curseur, p. ex. `item 0x0B2C "chair"`.
- **Check** vérifie le fichier : format incorrect, graphiques sans image, doublons. Les lignes fautives sont marquées en rouge.
- **Add selected tiles** ajoute les tuiles choisies dans l'onglet Items comme nouvelles lignes, chacune avec une ligne de commentaire portant le nom. Les tuiles déjà présentes sont ignorées. Ensuite, régler les directions des sièges, le remplacement des saisons, le shader des lumières.
- **Clic droit dans la liste des objets → Add to ClassicUO file** fait la même chose depuis la liste (chair, cave, vegetation, tree, seasons, lights). Dans la liste du terrain : **Add to seasons.txt**.
- **Save Txt** enregistre. Avant le premier enregistrement d'un fichier, `<fichier>.txt.bak` est créé. Une confirmation est demandée en cas d'erreurs de format.
- **Reload** recharge le fichier, **Open folder** ouvre le dossier.
- Changer de fichier avec des modifications non enregistrées demande d'abord confirmation.

ClassicUO lit les fichiers au démarrage. Redémarrer le client après l'enregistrement.

## Autres fonctions

- **Modèles** (liste déroulante de la barre) : valeurs typiques pour mur, porte, escalier, chaise … Les modèles de terrain (« LandTile … ») ne changent que les flags du terrain.
- **Copy / Insert settings** (clic droit) : copier les valeurs d'une tuile vers une autre. Avec plusieurs tuiles choisies, cliquer ensuite **Save changes**.
- **Mémoriser / Push marked** : mémoriser des valeurs et les transférer à d'autres objets avec le bouton du milieu ou Ctrl+Y.
- **Import / export CSV** : tous les objets ou tuiles de terrain sous forme de tableau. L'import peut être annulé.
- **Set filter** : filtrer les listes par valeurs et flags.
- **Set textures** : définir automatiquement les ids de texture du terrain.
