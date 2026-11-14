using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.IO;

public class GameManager : MonoBehaviour
{
    [Header("Composants UI")]
    public Transform grilleParent;
    public GameObject casePrefab;
    public TextMeshProUGUI texteScoreJ1;
    public TextMeshProUGUI texteScoreJ2;
    public TextMeshProUGUI texteParam; 
    public TextMeshProUGUI texteNJ1; 
    public TextMeshProUGUI texteNJ2; 
    public TextMeshProUGUI texteTaille;
    public TextMeshProUGUI texteTaille2;
    public TextMeshProUGUI texteGain;
    public TextMeshProUGUI texteGain2;
    public TextMeshProUGUI texteVictoire;      
    public TextMeshProUGUI textePivot;
    public TextMeshProUGUI textePivot2;
    public TextMeshProUGUI texteIA;
    public TextMeshProUGUI texteIA2;
    public RectTransform indictour;
    public GameObject appliquer;

    public GameObject settings; 
    public RectTransform menuparametres;
    public RectTransform menuvictoire;
    public Image mode1j;
    public Image mode2j;
    public Image moderapide;
    public TextMeshProUGUI textemode;

    [Header("Paramètres de Jeu")]
    public int mode = 1;
    public int taille = 6;
    public int gain = 4;
    public int pivot = 2;
    public int IA = 1;

    public int nbIA=13;

    [Header("Couleurs")]
    public Color colorJ1;
    public Color colorJ1A;
    public Color colorJ2;
    public Color colorJ2A;
    public Color colorVide;
    public Color colorWhite;

    private int[,] grilleLogique;
    private int player; 
    private int tour = 1;
    private int pts1 = 0;
    private int pts2 = 0;
    private bool isRotating = false;
    private bool jeuActif = true;

    public static Dictionary<(int x, int y), int> allignements = new Dictionary<(int x, int y), int>();
    private Dictionary<string, int> statsRarete = new Dictionary<string, int>();
    public static Dictionary<string, int> griddexCollection = new Dictionary<string, int>();    System.Random rand = new System.Random();
    public enum Rarete { Commun, Normal, Rare, Legendaire, Inconnu }
    // --- INITIALISATION ---

    void Awake() {
        if (mode1j != null && mode1j.TryGetComponent<Button>(out Button btn1j)) btn1j.onClick.AddListener(Mode1j);
        if (mode2j != null && mode2j.TryGetComponent<Button>(out Button btn2j)) btn2j.onClick.AddListener(Mode2j);
        if (moderapide != null && moderapide.TryGetComponent<Button>(out Button btnrapide)) btnrapide.onClick.AddListener(Moderapide);
        if (settings != null && settings.TryGetComponent<Button>(out Button btnSet)) btnSet.onClick.AddListener(ToggleMenu);
        if (menuvictoire != null && menuvictoire.TryGetComponent<Button>(out Button btnSet2)) btnSet2.onClick.AddListener(InitGrille);

        PreparerBoutonReglage(texteTaille, ModifierTaille);
        PreparerBoutonReglage(texteGain, ModifierGain);
        PreparerBoutonReglage(textePivot, ModifierPivot);
        PreparerBoutonReglage(texteIA, ModifierIA);
    }

void Start() {
    ChargerCollection();
    Moderapide(); // Ceci fixe taille = 4 et pivot = 1
    InitialiserBaseDeDonneesRarete(); 
}

    public void InitGrille() {
        jeuActif = true;
        isRotating = false;
        StopAllCoroutines(); 
        grilleParent.rotation = Quaternion.identity;
        texteNJ1.text = "";
        texteNJ2.text = "";
        if (menuvictoire != null) menuvictoire.anchoredPosition = new Vector2(-2000f, 0f);
        if (menuparametres != null) menuparametres.anchoredPosition = new Vector2(-3000f, -550f);
        RectTransform rtset = settings.GetComponent<RectTransform>();
        rtset.anchoredPosition=new Vector2(-300f,-550f);
        if (texteTaille != null) texteTaille.text = taille.ToString();
        if (texteGain != null) texteGain.text = gain.ToString();
        if (textePivot != null) textePivot.text = pivot.ToString();
        if (mode == 1)
        {
            if (texteIA != null) texteIA.text = IA.ToString();
        }
        tour = 1;
        player = (Random.value > 0.5f) ? -1 : 1;
        if (indictour != null) indictour.anchoredPosition = new Vector2(-200 * player, 650);
        pts1 = 0;
        pts2 = 0;

        if (texteParam != null) {
            if (mode==2){
                texteParam.text = $"Grille {taille}x{taille} | Serie de {gain} | Pivot de {pivot}";
            }
            if (mode==1){
                texteParam.text = $"IA niveau {IA} | Grille {taille}x{taille} | Serie de {gain} | Pivot de {pivot}";
            }
        }
        MettreAJourScores();
        grilleLogique = new int[taille, taille];

        foreach (Transform child in grilleParent) Destroy(child.gameObject);

        if (grilleParent.TryGetComponent<GridLayoutGroup>(out GridLayoutGroup gridLayout)) {
            gridLayout.enabled = true;
            gridLayout.constraintCount = taille;
            float size = 700f / taille; 
            gridLayout.cellSize = new Vector2(size, size);
        }

        for (int i = 0; i < taille; i++) {
            for (int j = 0; j < taille; j++) {
                GameObject nouvelleCase = Instantiate(casePrefab, grilleParent);
                nouvelleCase.name = i + "," + j;
                nouvelleCase.GetComponent<Image>().color = colorVide;
                int col = j; 
                if (nouvelleCase.TryGetComponent<Button>(out Button b)) {
                    b.onClick.AddListener(() => CliquerColonne(col, true));
                }
            }
        }
        if (jeuActif && mode == 1 && player == -1) {
            StartCoroutine(AttendreIA());
        }
        StartCoroutine(FixerLayoutApresFrame(gridLayout));
    }

    // --- LOGIQUE DE JEU ET DOUBLE GRAVITÉ ---

    void CliquerColonne(int col, bool estHumain = false) {
        if (estHumain && (mode == 1 && player != 1)) return;
        if (!jeuActif || isRotating || grilleLogique[0, col] != 0) return;

        StartCoroutine(SequenceDeCoup(col));
    }

    IEnumerator SequenceDeCoup(int col) {
        isRotating = true;

        // 1. Placement du pion
        grilleLogique[0, col] = player;
        ActualiserTouteLaGrille();
        yield return new WaitForSeconds(0.1f);

        // 2. PREMIÈRE GRAVITÉ (Le pion tombe dans sa colonne avant la rotation)
        yield return StartCoroutine(AnimerGravite());

        // 3. ROTATION (Si applicable)
        if (col < pivot) {
            yield return StartCoroutine(AnimerRotation(90));
        } else if (col >= taille - pivot) {
            yield return StartCoroutine(AnimerRotation(-90));
        } else {
            FinDeTour();
        }

        isRotating = false;
    }

    IEnumerator AnimerGravite() {
        bool mouvementAeuLieu = true;
        while (mouvementAeuLieu) {
            mouvementAeuLieu = false;
            for (int j = 0; j < taille; j++) {
                for (int i = taille - 2; i >= 0; i--) {
                    if (grilleLogique[i, j] != 0 && grilleLogique[i + 1, j] == 0) {
                        grilleLogique[i + 1, j] = grilleLogique[i, j];
                        grilleLogique[i, j] = 0;
                        mouvementAeuLieu = true;
                    }
                }
            }
            if (mouvementAeuLieu) {
                ActualiserTouteLaGrille();
                yield return new WaitForSeconds(0.06f);
            }
        }
    }

    IEnumerator AnimerRotation(float angle) {
        yield return new WaitForSeconds(0.2f);
        Quaternion depart = grilleParent.rotation;
        Quaternion arrivee = grilleParent.rotation * Quaternion.Euler(0, 0, angle);
        float temps = 0;
        while (temps < 1.0f) {
            temps += Time.deltaTime / 0.6f;
            grilleParent.rotation = Quaternion.Lerp(depart, arrivee, temps);
            yield return null;
        }

        if (angle > 0) RotationLogiqueGauche(); else RotationLogiqueDroite();
        
        grilleParent.rotation = Quaternion.identity; 
        ActualiserTouteLaGrille();

        // 4. DEUXIÈME GRAVITÉ (Le pion retombe après le pivot)
        yield return StartCoroutine(AnimerGravite());
        
        FinDeTour();
    }

    // --- GESTION DES TOURS ET IA ---

    void FinDeTour() {
        pts1 += CalculerAlignements(1,grilleLogique);
        pts2 += CalculerAlignements(-1,grilleLogique);
        MettreAJourScores();
        ActualiserTouteLaGrille();

        tour++;
        if (tour >= 2) {
            player *= -1;
            tour = 0;
            if (indictour != null) indictour.anchoredPosition = new Vector2(-200 * player, 650);
        }

        VerifFin();

        if (jeuActif && mode == 1 && player == -1) {
            StartCoroutine(AttendreIA());
        }
    }

    IEnumerator AttendreIA() {
        yield return new WaitForSeconds(0.8f);
        CoupIA(IA);
    }

    void CoupIA(int n) {
        if (!jeuActif) return;
        List<int> colonnesLibres = new List<int>();
        for (int k = 0; k < taille; k++) {
            if (grilleLogique[0, k] == 0) colonnesLibres.Add(k);
        }
        if (colonnesLibres.Count > 0) {
            if (n == 1)
        {
            int choix = colonnesLibres[rand.Next(0, colonnesLibres.Count)];
            CliquerColonne(choix, false); 
        }
            if (n >= 2  && n<=7)
            {
                int al=rand.Next(1,6);
                if (al < n)
                {
                    int choix=PrevisionMeilleurCoup1(colonnesLibres);
                    CliquerColonne(choix, false); 
                }
                else
                {
                    int choix = colonnesLibres[rand.Next(0, colonnesLibres.Count)];
                    CliquerColonne(choix, false); 
                }
            }
            if (n>=8 && n <= 13)
            {
                int al=rand.Next(1,6);
                if (al+6 < n)
                {
                    int choix=PrevisionMeilleurCoup2(colonnesLibres);
                    CliquerColonne(choix, false); 
                }
                else
                {
                    int choix=PrevisionMeilleurCoup1(colonnesLibres);
                    CliquerColonne(choix, false); 
                }
            }
        }
    }

// prédit le meilleur coup parmis les 6 possibles
int PrevisionMeilleurCoup1(List<int> cl)
{
    int maxi=-1000;
    int res=-1;
    foreach (int c in cl)
    {
        int diff=PrevisionGainPoint1(c,-1);
        if (diff > maxi)
        {
            maxi=diff;
            res=c;
        }
    }
    return res;
}
// prédit les 2 meilleurs coups parmis les 36 possibles
int PrevisionMeilleurCoup2(List<int> cl)
{
    float meilleurScoreMoyen = -10000f;
    int meilleurC1 = cl[rand.Next(cl.Count)];

    foreach (int c1 in cl)
    {
        // 1. Simuler le premier coup pour obtenir une grille intermédiaire
        int[,] grilleApresC1 = SimulerCoupComplet(grilleLogique, c1, -1);
        
        float sommeScoresC2 = 0;

        foreach (int c2 in cl)
        {
            // 2. Simuler le deuxième coup sur la grille modifiée par c1
            int[,] grilleApresC2 = SimulerCoupComplet(grilleApresC1, c2, -1);
            
            // 3. Évaluer la grille finale
            int pointsIA = CalculerAlignementsSimu(-1, grilleApresC2);
            int pointsJoueur = CalculerAlignementsSimu(1, grilleApresC2);
            sommeScoresC2 += (pointsIA - pointsJoueur);
        }

        float scoreMoyenPourC1 = sommeScoresC2 / cl.Count;

        if (scoreMoyenPourC1 > meilleurScoreMoyen)
        {
            meilleurScoreMoyen = scoreMoyenPourC1;
            meilleurC1 = c1;
        }
    }
    return meilleurC1;
}
int PrevisionGainPoint1(int c, int joueur){
    // On utilise notre simulateur robuste pour voir l'état de la grille après ce coup
    int[,] grilleTest = SimulerCoupComplet(grilleLogique, c, joueur);

    // Calcul du différentiel de points (Points IA - Points Joueur)
    int pointsIA = CalculerAlignementsSimu(-1, grilleTest);
    int pointsJoueur = CalculerAlignementsSimu(1, grilleTest);
    
    return pointsIA - pointsJoueur;
}
// Fonction utilitaire pour simuler un placement + gravité + rotation + gravité
int[,] SimulerCoupComplet(int[,] grilleOrigine, int col, int idJoueur)
{
    // On récupère la taille réelle de la grille passée en paramètre
    int tLocal = grilleOrigine.GetLength(0);
    int[,] g = (int[,])grilleOrigine.Clone();
    
    // Placement du pion
    int ligne = -1;
    for (int i = tLocal - 1; i >= 0; i--) {
        if (g[i, col] == 0) {
            ligne = i;
            break;
        }
    }

    if (ligne != -1) {
        g[ligne, col] = idJoueur;

        // Rotation (on utilise le pivot actuel du jeu)
        if (col < pivot) {
            g = SimulerRotation(g, true);
        } else if (col >= tLocal - pivot) {
            g = SimulerRotation(g, false);
        }
    }
    
    return g;
}

int[,] SimulerRotation(int[,] source, bool gauche) {
    int[,] nG = new int[taille, taille];
    // Rotation pure
    for (int i = 0; i < taille; i++) {
        for (int j = 0; j < taille; j++) {
            if (gauche) nG[taille - 1 - j, i] = source[i, j];
            else nG[j, taille - 1 - i] = source[i, j];
        }
    }
    // Application de la gravité après rotation
    for (int j = 0; j < taille; j++) {
        List<int> pions = new List<int>();
        for (int i = 0; i < taille; i++) {
            if (nG[i, j] != 0) pions.Add(nG[i, j]);
        }
        for (int i = 0; i < taille; i++) {
            int indexInverse = taille - 1 - i;
            if (i < pions.Count) nG[indexInverse, j] = pions[pions.Count - 1 - i];
            else nG[indexInverse, j] = 0;
        }
    }
    return nG;
}
int CalculerAlignementsSimu(int n, int[,] grille) {
    int total = 0;
    for (int i = 0; i < taille; i++)
        for (int j = 0; j <= taille - gain; j++)
            if (CheckLigneSimu(i, j, 0, 1, n, grille)) total++;
            for (int i = 0; i <= taille - gain; i++)
            for (int j = 0; j < taille; j++)
                if (CheckLigneSimu(i, j, 1, 0, n,grille)) total++;
        for (int i = 0; i <= taille - gain; i++) {
            for (int j = 0; j <= taille - gain; j++)
                if (CheckLigneSimu(i, j, 1, 1, n,grille)) total++;
            for (int j = gain - 1; j < taille; j++)
                if (CheckLigneSimu(i, j, 1, -1, n,grille)) total++;
        }   
    return total;
}

bool CheckLigneSimu(int l, int c, int dl, int dc, int n, int[,] grille) {
    for (int k = 0; k < gain; k++)
        if (grille[l + k * dl, c + k * dc] != n) return false;
    return true;
}
    // --- RÉGLAGES ORIGINAUX ---

    public void AppliquerNouveauxReglages() {
        taille = int.Parse(texteTaille.text);
        gain = int.Parse(texteGain.text);
        pivot = int.Parse(textePivot.text);
        IA=int.Parse(texteIA.text);
        InitGrille();
    }

    void ModifierIA() {
        int t = int.Parse(texteIA.text);
        t = (t >= nbIA) ? 1 : t + 1;
        texteIA.text = t.ToString();
    }
    void ModifierTaille() {
        int t = int.Parse(texteTaille.text);
        t = (t >= 9) ? 5 : t + 1;
        texteTaille.text = t.ToString();
        texteGain.text = "3";
        textePivot.text = "1";
    }

    void ModifierGain() {
        int g = int.Parse(texteGain.text);
        int maxGain = int.Parse(texteTaille.text) - 2;
        g = (g >= maxGain) ? 3 : g + 1;
        texteGain.text = g.ToString();
    }

    void ModifierPivot() {
        int p = int.Parse(textePivot.text);
        int maxPivot = int.Parse(texteTaille.text) / 2 - 1;
        p = (p >= maxPivot) ? 1 : p + 1;
        textePivot.text = p.ToString();
    }

    // --- LOGIQUE TECHNIQUE ---

    void RotationLogiqueGauche() {
        int[,] nG = new int[taille, taille];
        for (int i = 0; i < taille; i++)
            for (int j = 0; j < taille; j++)
                nG[taille - 1 - j, i] = grilleLogique[i, j];
        grilleLogique = nG;
    }

    void RotationLogiqueDroite() {
        int[,] nG = new int[taille, taille];
        for (int i = 0; i < taille; i++)
            for (int j = 0; j < taille; j++)
                nG[j, taille - 1 - i] = grilleLogique[i, j];
        grilleLogique = nG;
    }

    void ActualiserTouteLaGrille() {
        for (int i = 0; i < taille; i++) {
            for (int j = 0; j < taille; j++) {
                Transform caseT = grilleParent.Find(i + "," + j);
                if (caseT != null && caseT.TryGetComponent<Image>(out Image img)) {
                    int val = grilleLogique[i, j];
                    if (allignements.ContainsKey((i,j)))
                        img.color = (val == 1) ? colorJ1A : colorJ2A ;
                    else
                        img.color = (val == 1) ? colorJ1 : (val == -1) ? colorJ2 : colorVide;
                }
            }
        }
        allignements = new Dictionary<(int x, int y), int>();
    }

    int CalculerAlignements(int n,int[,]grille) {
        int total = 0;
        for (int i = 0; i < taille; i++)
            for (int j = 0; j <= taille - gain; j++)
                if (CheckLigne(i, j, 0, 1, n,grille)) total++;
        for (int i = 0; i <= taille - gain; i++)
            for (int j = 0; j < taille; j++)
                if (CheckLigne(i, j, 1, 0, n,grille)) total++;
        for (int i = 0; i <= taille - gain; i++) {
            for (int j = 0; j <= taille - gain; j++)
                if (CheckLigne(i, j, 1, 1, n,grille)) total++;
            for (int j = gain - 1; j < taille; j++)
                if (CheckLigne(i, j, 1, -1, n,grille)) total++;
        }
        if (n == 1) texteNJ1.text = total == 0 ? "" : "+" + total;
        else texteNJ2.text = total == 0 ? "" : "+" + total;
        if (mode==3 && n==1) pts1=total;
        if (mode==3 && n==-1) pts2=total;
        return total;
    }

    bool CheckLigne(int l, int c, int dl, int dc, int n,int[,]grille) {
        for (int k = 0; k < gain; k++)
            if (grille[l + k * dl, c + k * dc] != n) return false;
        DicoAllignement(l, c, dl, dc, n);
        return true;
    }

    void DicoAllignement(int l, int c, int dl, int dc, int n) {
        for (int k = 0; k < gain; k++)
            if (!allignements.ContainsKey((l + k * dl, c + k * dc)))
                allignements[(l + k * dl, c + k * dc)] = n;
    }

    void VerifFin() {
        bool grillePleine = true;
        for (int k = 0; k < taille; k++) { if (grilleLogique[0, k] == 0) grillePleine = false; }
        if (grillePleine) {
            jeuActif = false;
            if (mode == 3) VerifierEtAjouterAuGriddex();
            if (menuvictoire != null) {
                menuvictoire.anchoredPosition = new Vector2(0, 0);
                if (pts1 > pts2) { texteVictoire.text = "Le joueur 1 a gagne !"; texteVictoire.color = colorJ1; }
                else if (pts2 > pts1) { texteVictoire.color = colorJ2; texteVictoire.text = mode == 1 ? "L'IA a gagne !" : "Le joueur 2 a gagne !"; }
                else { texteVictoire.text = "Egalite !"; texteVictoire.color = colorVide; }
            }
        }
    }

    void PreparerBoutonReglage(TextMeshProUGUI texteUI, UnityEngine.Events.UnityAction action) {
        if (texteUI != null && texteUI.TryGetComponent<Button>(out Button btn)) {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(action);
        }
    }

    void MettreAJourScores() {
        if (texteScoreJ1 != null) texteScoreJ1.text = pts1.ToString();
        if (texteScoreJ2 != null) texteScoreJ2.text = pts2.ToString();
    }

    public void ToggleMenu() {
        if (menuparametres != null)
        {
            bool estCache = menuparametres.anchoredPosition.x < -1000;
            if (mode == 2)
            {
                menuparametres.anchoredPosition = new Vector2(estCache ? -430f : -3000f, -550f);
                menuparametres.sizeDelta = new Vector2(520, 160);
                texteIA.rectTransform.anchoredPosition=new Vector2(-3000f,-550f);
                texteIA2.rectTransform.anchoredPosition=new Vector2(-3000f,-550f);
                texteTaille.rectTransform.anchoredPosition=new Vector2(-190f,-5f);
                texteTaille2.rectTransform.anchoredPosition=new Vector2(-190f,40f);
                texteGain.rectTransform.anchoredPosition=new Vector2(-90f,-5f);
                texteGain2.rectTransform.anchoredPosition=new Vector2(-90f,40f);
                textePivot.rectTransform.anchoredPosition=new Vector2(10f,-5f);
                textePivot2.rectTransform.anchoredPosition=new Vector2(10f,40f);
                RectTransform rt = appliquer.GetComponent<RectTransform>();
                rt.anchoredPosition=new Vector2(165f,0f);
                RectTransform rtset = settings.GetComponent<RectTransform>();
                rtset.anchoredPosition=new Vector2(-300f,-550f);
            }
            else if (mode == 1)
            {
                menuparametres.anchoredPosition = new Vector2(estCache ? -470f : -3000f, -550f);
                menuparametres.sizeDelta = new Vector2(620, 160);
                texteIA.rectTransform.anchoredPosition=new Vector2(-240f,-5f);
                texteIA2.rectTransform.anchoredPosition=new Vector2(-240f,40f);
                texteTaille.rectTransform.anchoredPosition=new Vector2(-140f,-5f);
                texteTaille2.rectTransform.anchoredPosition=new Vector2(-140f,40f);
                texteGain.rectTransform.anchoredPosition=new Vector2(-40f,-5f);
                texteGain2.rectTransform.anchoredPosition=new Vector2(-40f,40f);
                textePivot.rectTransform.anchoredPosition=new Vector2(60f,-5f);
                textePivot2.rectTransform.anchoredPosition=new Vector2(60f,40f);
                RectTransform rt = appliquer.GetComponent<RectTransform>();
                rt.anchoredPosition=new Vector2(215f,0f);
                if (estCache)
                {
                    RectTransform rtset = settings.GetComponent<RectTransform>();
                    rtset.anchoredPosition=new Vector2(-350f,-550f);
                }
                else
                {
                    RectTransform rtset = settings.GetComponent<RectTransform>();
                    rtset.anchoredPosition=new Vector2(-300f,-550f);
                }


            }
        }
  
    }

    void Mode1j() { mode = 1; mode1j.color = colorJ1; mode2j.color = colorWhite; moderapide.color = colorWhite; textemode.text = "SOLO"; taille=6; gain=4; pivot=2; InitGrille(); }

    void Moderapide() { mode = 3; moderapide.color = colorJ1; mode2j.color = colorWhite; mode1j.color = colorWhite;  textemode.text = "RAPIDE"; taille=4; gain=3; pivot=1; InitGrille(); }

    void Mode2j() { mode = 2; mode2j.color = colorJ1; mode1j.color = colorWhite; moderapide.color = colorWhite; textemode.text = "2 JOUEURS";RectTransform rtset = settings.GetComponent<RectTransform>(); rtset.anchoredPosition=new Vector2(-300f,-550f);taille=6; gain=4; pivot=2; InitGrille(); }
    IEnumerator FixerLayoutApresFrame(GridLayoutGroup gl) { yield return new WaitForEndOfFrame(); if (gl != null) gl.enabled = false; }


bool VerifierCritere(int[,] grille)
{
    // Exemple : Chercher si le centre est rempli de pions "1"
    if (grille[1,1] == 1 && grille[1,2] == 1 && grille[2,1] == 1 && grille[2,2] == 1)
        return true;

    return false;
}
void VerifierEtAjouterAuGriddex() {
    if (mode != 3) return; // Uniquement en mode Rapide

    string signature = GenererSignature(grilleLogique);
    
    if (!griddexCollection.ContainsKey(signature)) {
        // La grille est nouvelle ! 
        griddexCollection.Add(signature, 1);
        Rarete r = DeterminerRarete(signature);
        Debug.Log($"<color=gold>NOUVELLE GRILLE DÉCOUVERTE !</color> Rareté : {r}");
        // Optionnel : Afficher un petit effet visuel ou texte à l'écran
    }
}

Rarete DeterminerRarete(string signature) {
    if (!statsRarete.ContainsKey(signature)) return Rarete.Legendaire;

    int nbApparitions = statsRarete[signature];
    
    if (nbApparitions > 9) return Rarete.Commun;    // ~0.5% de chance
    if (nbApparitions > 3)  return Rarete.Normal;    // ~0.1%
    if (nbApparitions > 1)  return Rarete.Rare;      // ~0.02%
    return Rarete.Legendaire;                         // Ultra rare
}
public void LancerSimulationPourGriddex(int nombreDeParties)
{
    statsRarete.Clear();
    int tSimu = 4; // On force la simulation en 4x4 pour le Griddex

    for (int n = 0; n < nombreDeParties; n++)
    {
        List<int> stock = new List<int>();
        for (int i = 0; i < 8; i++) { stock.Add(1); stock.Add(-1); }
        
        int[,] grilleSimu = new int[tSimu, tSimu];

        while (stock.Count > 0)
        {
            int index = Random.Range(0, stock.Count);
            int pion = stock[index];
            
            // Random.Range(0, 4) donne 0, 1, 2 ou 3
            int col = Random.Range(0, tSimu); 
            
            // On vérifie si la colonne choisie n'est pas déjà pleine
            if (grilleSimu[0, col] == 0) {
                grilleSimu = SimulerCoupComplet(grilleSimu, col, pion);
                stock.RemoveAt(index);
            }
            // Si la colonne est pleine, la boucle while continue et choisira une autre colonne
        }

        string sig = GenererSignature(grilleSimu);
        if (statsRarete.ContainsKey(sig)) statsRarete[sig]++;
        else statsRarete.Add(sig, 1);
    }
}
string GenererSignature(int[,] grille) {
    System.Text.StringBuilder sb = new System.Text.StringBuilder();
    // On s'adapte dynamiquement à la taille (4x4 ici)
    int size = grille.GetLength(0); 
    for (int i = 0; i < size; i++) {
        for (int j = 0; j < size; j++) {
            int val = grille[i, j];
            sb.Append(val == -1 ? 2 : val); 
        }
    }
    return sb.ToString();
}
public void InitialiserBaseDeDonneesRarete() {
    Debug.Log("Génération de la base de données de rareté...");
    LancerSimulationPourGriddex(50000); 
}

public void SauvegarderCollection() {
    // On transforme le dictionnaire en une liste d'objets pour le JSON
    string json = JsonUtility.ToJson(new SerializationWrapper<string, int>(griddexCollection));
    File.WriteAllText(Application.persistentDataPath + "/griddex.json", json);
    Debug.Log("Collection sauvegardée !");
}

public void ChargerCollection() {
    string chemin = Application.persistentDataPath + "/griddex.json";
    if (File.Exists(chemin)) {
        string json = File.ReadAllText(chemin);
        // Ici, tu récupères tes données et tu remplis griddexCollection
        Debug.Log("Collection chargée !");
    }
}

// Classe utilitaire pour permettre à Unity de sérialiser un Dictionnaire
[System.Serializable]
public class SerializationWrapper<TKey, TValue> {
    public List<TKey> Keys = new List<TKey>();
    public List<TValue> Values = new List<TValue>();
    public SerializationWrapper(Dictionary<TKey, TValue> dico) {
        foreach (var kvp in dico) { Keys.Add(kvp.Key); Values.Add(kvp.Value); }
    }
}
}