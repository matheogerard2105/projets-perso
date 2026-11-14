# Relevé Terrain QR

Application web mobile que j'ai développée pour aider mon père dans son travail de technicien :
faire l'inventaire des équipements installés sur un chantier (par exemple les têtes
thermostatiques connectées de chaque chambre d'un bâtiment) en scannant leur QR code, au lieu de
tout noter à la main puis de le recopier dans un tableur.

## À quoi elle sert

Sur un chantier, le technicien doit vérifier que chaque équipement prévu est bien posé, à quel
endroit, et noter son numéro de série. L'application lui permet de :

1. **Importer la liste des équipements attendus** depuis un fichier Excel (site, étage, local,
   type d'équipement, quantité).
2. **Scanner le QR code** de chaque équipement avec l'appareil photo du téléphone (ou depuis une
   photo, ou en saisissant l'identifiant à la main si le QR code est illisible).
3. **Remplir la fiche du relevé** : date, étage, local, catégorie, numéro de série, commentaire,
   photo de l'équipement et signature facultative. Un QR code déjà relevé est signalé.
4. **Suivre l'avancement** : tableau de bord par étage et par local, avec le nombre
   d'équipements relevés et restants par rapport à la liste attendue.
5. **Exporter tous les relevés vers Excel** pour le compte rendu de fin de chantier.

## Fonctionnement technique

- Un seul fichier `index.html`, sans serveur ni installation : il suffit de l'ouvrir dans le
  navigateur du téléphone.
- Les données restent sur l'appareil, dans **IndexedDB** : l'application fonctionne sans connexion
  une fois chargée, ce qui est utile en sous-sol ou dans des bâtiments mal couverts.
- Lecture des QR codes avec la caméra (`getUserMedia`) et la bibliothèque **jsQR**.
- Import et export Excel avec **SheetJS** (`xlsx`).

## Utilisation

Ouvrir `index.html` dans un navigateur récent (Chrome ou Safari sur mobile). L'accès à la caméra
nécessite que la page soit servie en HTTPS ou ouverte en local.
