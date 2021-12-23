using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum TurnPhase
{
    idle,
    pre,
    waiting,
    post,
    gameOver
}

public class Bartok : MonoBehaviour
{
    static public Bartok S;
    private BartokLayout layout;
    private Transform layoutAnchor;
    public static Player CURRENT_PLAYER;

    [Header("Set in Inspector")]
    public TextAsset deckXML;
    public TextAsset layoutXML;
    public Vector3 layoutCenter = Vector3.zero;
    public float handFanDegrees = 20f;
    public int numStartingCards = 6;
    public float drawTimeStagger = 0.1f;

    [Header("Set Dynamically")]
    public Deck deck;
    public List<CardBartok> drawPile;
    public List<CardBartok> discardPile;
    public List<Player> players;
    public CardBartok targetcard;
    public TurnPhase phase = TurnPhase.idle;
    public static readonly int players_ = 2;

    void Awake()
    {
        S = this;
    }

    void Start()
    {
        deck = GetComponent<Deck>();
        deck.InitDeck(deckXML.text);
        Deck.Shuffle(ref deck.cards);

        layout = GetComponent<BartokLayout>();
        layout.ReadLayout(layoutXML.text);
        drawPile = UpgradeCardsList(deck.cards);

        LayoutGame();
    }
    public void ArrangeDrawPile()
    {
        CardBartok tCB;
        for (int i = 0; i < drawPile.Count; i++)
        {
            tCB = drawPile[i];
            tCB.transform.SetParent(layoutAnchor);
            tCB.transform.localScale = new Vector3(1.2f, 1.2f, 0f);
            tCB.transform.localPosition = layout.drawPile.pos;
            tCB.faceUp = false;
            tCB.SetSortingLayerName(layout.drawPile.layerName);
            tCB.SetSortOrder(-i * players_);
            tCB.state = CBState.drawpile;
        }
    }
    private void LayoutGame()
    {
        if (layoutAnchor == null)
        {
            GameObject tGO = new GameObject("_LayoutAnchor");
            layoutAnchor = tGO.transform;
            layoutAnchor.transform.position = layoutCenter;
        }
        ArrangeDrawPile();
        Player pl;
        players = new List<Player>();
        foreach (SlotDef tSD in layout.slotDefs)
        {
            pl = new Player();
            pl.handSlotDef = tSD;
            players.Add(pl);
            pl.playerNum = tSD.player;
        }
        players[0].type = PlayerType.human;

        CardBartok tCB;
        for (int i = 0; i < players_; i++)
        {
            for (int j = 0; j < numStartingCards; j++)
            {
                tCB = Draw();

                tCB.timeStart = Time.time + drawTimeStagger * (i * players_ + j);
                players[(j + 1) % players_].AddCard(tCB);
            }
        }

        Invoke("DrawFirstTarget", drawTimeStagger * (numStartingCards * players_ + players_));
    }
    public void DrawFirstTarget()
    {
        CardBartok tCB = MoveToTarget(Draw());
        tCB.reportFinishTo = this.gameObject;
    }

    public void CBCallback(CardBartok cb)
    {
        Utils.tr("Bartok:CBCallback()", cb.name);
        StartGame();
    }
    public void StartGame()
    {
        PassTurn(1);
    }

    public void PassTurn(int num = -1)
    {
        if (num == -1)
        {
            int ndx = players.IndexOf(CURRENT_PLAYER);
            num = (ndx + 1) % players_;
        }
        int lastPlayerNum = -1;
        if (CURRENT_PLAYER != null)
        {
            lastPlayerNum = CURRENT_PLAYER.playerNum;
            if (CheckGameOver())
                return;
        }
        CURRENT_PLAYER = players[num];
        phase = TurnPhase.pre;
        GameObject.Find("TurnLight").GetComponent<Light>().range = 12;
        CURRENT_PLAYER.TakeTurn();
        Utils.tr("Bartok:PassTurn()", "Old: " + lastPlayerNum, "New: " + CURRENT_PLAYER.playerNum);
    }

    private bool CheckGameOver()
    {
        if (drawPile.Count == 0)
        {
            List<Card> cards = new List<Card>();
            foreach (CardBartok cb in discardPile)
                cards.Add(cb);
            discardPile.Clear();
            Deck.Shuffle(ref cards);
            drawPile = UpgradeCardsList(cards);
            ArrangeDrawPile();
        }
        if (CURRENT_PLAYER.hand.Count == 0)
        {
            phase = TurnPhase.gameOver;
            Invoke("RestartGame", 1);
            return (true);
        }
        return false;
    }

    public void RestartGame()
    {
        CURRENT_PLAYER = null;
        SceneManager.LoadScene("__Bartok_Scene_0");
    }

    public bool ValidPlay(CardBartok cb)
    {
        if (cb.suit != targetcard.suit)
            return (true);

        return (false);
    }

    public CardBartok MoveToTarget(CardBartok tCB)
    {
        tCB.timeStart = 0;
        tCB.MoveTo(layout.discardPile.pos + Vector3.back);
        tCB.state = CBState.toTarget;
        tCB.faceUp = true;
        tCB.SetSortingLayerName("10");
        tCB.eventualSortLayer = layout.target.layerName;
        if (targetcard != null)
            MoveToDiscard(targetcard);
        targetcard = tCB;
        return (tCB);
    }
    public CardBartok MoveToDiscard(CardBartok tCB)
    {
        tCB.state = CBState.discard;
        discardPile.Add(tCB);
        tCB.SetSortingLayerName(layout.discardPile.layerName);
        tCB.SetSortOrder(discardPile.Count * players_);
        tCB.transform.localPosition = layout.discardPile.pos + Vector3.back / 2;
        return (tCB);
    }
    public CardBartok Draw()
    {
        CardBartok cd = drawPile[0];
        if (drawPile.Count == 0)
        {
            int ndx;
            while (discardPile.Count > 0)
            {
                ndx = Random.Range(0, discardPile.Count);
                drawPile.Add(discardPile[ndx]);
                discardPile.RemoveAt(ndx);
            }
            ArrangeDrawPile();
            float t = Time.time;
            foreach (CardBartok tCB in drawPile)
            {
                tCB.transform.localPosition = layout.discardPile.pos;
                tCB.callbackPlayer = null;
                tCB.MoveTo(layout.drawPile.pos);
                tCB.timeStart = t;
                t += 0.02f;
                tCB.state = CBState.toDrawpile;
                tCB.eventualSortLayer = "0";
            }
        }
        drawPile.RemoveAt(0);
        return (cd);
    }

    List<CardBartok> UpgradeCardsList(List<Card> lCD)
    {
        List<CardBartok> lCB = new List<CardBartok>();
        foreach (Card tCD in lCD)
            lCB.Add(tCD as CardBartok);
        return lCB;
    }
    public void CardClicked(CardBartok tCB)
    {
        if (CURRENT_PLAYER.type == PlayerType.ai)
            return;
        if (phase == TurnPhase.waiting)
            return;
        if (!tCB.faceUp)
            return;
        if (ValidPlay(tCB))
        {
            CURRENT_PLAYER.RemoveCard(tCB);
            MoveToTarget(tCB);
            List<CardBartok> removeList = new List<CardBartok>();
            foreach (var card in CURRENT_PLAYER.hand)
            {
                if (card.rank == targetcard.rank)
                {
                    MoveToDiscard(card);
                    removeList.Add(card);
                }
            }
            tCB.callbackPlayer = CURRENT_PLAYER;
            phase = TurnPhase.waiting;
            if (removeList.Count != 0)
            {
                foreach (var item in removeList)
                    CURRENT_PLAYER.RemoveCard(item);
            }
            Utils.tr("Bartok:CardClicked()", "Play", tCB.name, targetcard.name + " is target");
        }
        else
            Utils.tr("Bartok:CardClicked()", "Attempted to Play", tCB.name, targetcard.name + " is target");
    }
}