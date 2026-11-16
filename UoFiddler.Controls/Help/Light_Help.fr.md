# Light – aide

L'onglet Light affiche et modifie les lumières de `light.mul` / `lightidx.mul`. Une lumière est une image en niveaux de gris : chaque pixel a une valeur de 0 (pas de lumière) à 31 (pleine luminosité). Le client la pose sur le monde quand un objet avec le flag *LightSource* éclaire.

Toutes les modifications se font d'abord en mémoire. Elles sont écrites avec **Save light.mul** (dossier de sortie).

## Liste et recherche

- La liste affiche `id (hex) – nom`.
- **Noms** : F2 ou clic droit → *Name…*. Les noms sont enregistrés dans `%AppData%\UoFiddler\LightNames.json`, pas dans le mul.
- **Recherche** en haut : id (`12`), hex (`0x0C`), plage (`10-20`) ou partie du nom.
- **▦ Overview** montre toutes les lumières en images. Un double-clic en choisit une.
- **Suppr** supprime la lumière, **F2** la renomme.

## Aperçu

- Normalement, la lumière s'affiche seule (noir = pas de lumière, blanc = pleine luminosité), avec zoom.
- **In-game preview** montre la lumière sur des tuiles de sol avec un objet au centre :
  - **Ground** : tuile de terrain du sol. **◀ ▶** parcourent, **▦ Overview…** choisit dans l'aperçu général.
  - **Light source** : l'objet du centre. ◀ ▶ parcourent, au choix seulement les objets avec le flag *LightSource*.
  - **Darkness %** : obscurité de la scène sans lumière.
  - **Light offset X/Y** : position de la lumière par rapport à l'objet.
  - **Light colour…** : tester une lumière colorée. ClassicUO peut colorer les lumières avec une hue dans `lights.txt`.
  - **Flicker** : la luminosité varie comme une flamme.
- Clic droit sur l'aperçu : copier l'aperçu ou l'enregistrer en PNG.

## Plus de 100 lumières

Les fichiers d'origine ont 100 entrées. Le nombre est lu dans `lightidx.mul`.

- **Entries: [ ] Set** agrandit les fichiers, p. ex. à 255, ou les raccourcit.
- Insérer une lumière après la fin agrandit les fichiers automatiquement.
- ClassicUO lit toutes les entrées de `lightidx.mul`. Pour les autres clients, testez vous-même s'ils utilisent plus de 100 entrées.
- La lumière utilisée par un objet est définie dans la tiledata.

## Clic droit dans la liste

| Entrée | Fonction |
|---|---|
| Export image | en PNG, BMP, TIFF, JPG ; ou toutes les lumières en PNG dans un dossier |
| Name… / Copy ID | nommer, copier l'id |
| Remove | supprimer la lumière |
| Replace from file… | charger une image (luminosité de l'image = valeur de lumière, noir = pas de lumière) |
| Insert at ID | taper l'id, Entrée, choisir une image |
| Copy to ID / Move to ID | copier ou déplacer la lumière (avec son nom) vers un autre id |
| Copy image / Import from clipboard | via le presse-papiers |
| Edit in light designer… | ouvre le concepteur de lumière avec cette lumière |

## Autres boutons

- **✔ Check…** trouve les lumières en double (même image), vides, très sombres (valeur max. sous 4), très grandes (plus de 512 pixels) et celles avec des valeurs « négatives » au-dessus de 31. Il indique aussi le nombre d'entrées libres.
- **Import folder…** lit toutes les images d'un dossier. L'id fait partie du nom du fichier : `Light 12 Bougie.png` → lumière 12 nommée « Bougie ». `12.png` et `0x0C.bmp` fonctionnent aussi. C'est la contrepartie de « Export → toutes les lumières ».

## Concepteur de lumière

Ouvrir avec **✚ New light…** (nouvelle lumière, premier id libre) ou **✎ Edit…** (lumière choisie).

### Créer une nouvelle lumière

- **Modèles** (double-clic) : bougie, lampe, grande lumière, torche, fenêtre (lumière vers la gauche/droite), spot, rayon, anneau.
- **Forme** : ronde, ovale, cône (spot), fenêtre (s'élargit), rayon (largeur constante), anneau.
- **Taille de l'image** et **position de la source** dans l'image.
- **Rayon**, **direction °** (0 = droite, 90 = bas, 180 = gauche, 270 = haut), **angle d'ouverture**, **longueur**, **largeur de l'ouverture**.
- **Luminosité** (1–31), **atténuation** (100 % = régulière, plus = s'assombrit plus vite), **bord doux**, **grain** (comme les originaux) et **motif du grain**.
- **Miroir ↔ ↕** et **rotation** 0/90/180/270°.

### À la souris

- **Glisser avec le bouton gauche** dans l'aperçu déplace la source de lumière (avec une image modifiée : toute l'image).
- **Glisser avec le bouton droit** déplace l'image guide.

### Image guide

Un objet (p. ex. une fenêtre), une tuile de terrain ou un fichier image est affiché par-dessus la lumière pour l'ajuster précisément. Position, taille et opacité sont réglables. L'image guide n'est **pas** enregistrée.

### Tester

- Vue **Light only** ou **In game** (sol, objet, obscurité, décalage de la lumière)
- **Colour…** pour une lumière colorée, **Flicker** pour le scintillement
- Maintenir **◑ Before** montre l'état à l'ouverture

### Modifier l'image finie

Rotation libre (° ; par pas de 90° sans perte), taille %, luminosité %, déplacement X/Y. Ensuite les champs de forme sont désactivés. **Back to the design** revient en arrière.

### Annuler

**↶ / ↷** ou Ctrl+Z / Ctrl+Y.

### Enregistrer

- **Into light.mul** : à l'id saisi. À côté s'affiche si l'id est libre ou sera remplacé. Ensuite **Save light.mul** dans l'onglet Light.
- **Export PNG…** enregistre l'image.
- **Save as .uolight…** enregistre conception et image dans un fichier. On le recharge avec **Import…** pour le modifier plus tard.
- **Import…** charge `.uolight`, PNG, BMP ou TIFF.
- **Liste du presse-papiers** :
  - **Keep** y dépose la lumière actuelle. La liste reste tant qu'UoFiddler tourne.
  - **Load** ou un double-clic la recharge.
  - **All into folder…** les enregistre toutes en `.uolight`.
  - **Load folder…** charge un dossier (par défaut `%AppData%\UoFiddler\LightLibrary`).
