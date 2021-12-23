using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum PlayerType
{
    human,
    ai,
    onHuman
}

public class Player
{
    public PlayerType type = PlayerType.ai;
    public int playerNum;
    public SlotDef handSlotDef;
    public List<CardBartok> hand;


    public CardBartok AddCard(CardBartok eCB)
    {
        if (hand == null)
            hand = new List<CardBartok>();
        hand.Add(eCB);
        if (type == PlayerType.human || type == PlayerType.onHuman)
        {
            CardBartok[] cards = hand.ToArray();
            cards = cards.OrderBy(cd => cd.rank).ToArray();
            hand = new List<CardBartok>(cards);
        }
        eCB.SetSortingLayerName("10");
        eCB.eventualSortLayer = handSlotDef.layerName;
        FanHand();
        return (eCB);
    }

    private void FanHand()
    {
        float startRot = handSlotDef.rot;
        if (hand.Count > 1)
            startRot += Bartok.S.handFanDegrees * (hand.Count - 1) / 2;
        Vector3 pos;
        float rot;
        Quaternion rotQ;
        for (int i = 0; i < hand.Count; i++)
        {
            rot = startRot - Bartok.S.handFanDegrees * i;
            rotQ = Quaternion.Euler(0, 0, rot);
            pos = Vector3.up * CardBartok.CARD_HEIGHT / 2f;
            pos = rotQ * pos;
            pos += handSlotDef.pos;
            pos.z = -0.5f * i;

            if (Bartok.S.phase != TurnPhase.idle)
                hand[i].timeStart = 0;

            hand[i].MoveTo(pos, rotQ);
            hand[i].state = CBState.toHand;

            hand[i].faceUp = type == PlayerType.human;
            hand[i].eventualSortOrder = i * 4;
        }
    }

    public CardBartok RemoveCard(CardBartok cb)
    {
        if (hand == null || !hand.Contains(cb))
            return null;
        hand.Remove(cb);
        FanHand();
        return (cb);
    }

    public void TakeTurn()
    {
        //If AI
        CardBartok cb;
        cb = AddCard(Bartok.S.Draw());
        List<CardBartok> validCards = new List<CardBartok>();
        foreach (CardBartok tCB in hand)
        {
            if (Bartok.S.ValidPlay(tCB))
                validCards.Add(tCB);
        }
        if (validCards.Count == 0)
        {
            foreach (var card in Bartok.S.discardPile)
                this.AddCard(card);
            Bartok.S.discardPile.Clear();
            cb.callbackPlayer = this;
            return;
        }
        if (type == PlayerType.human)
            return;
        Utils.tr("Player.TakeTurn");
        Bartok.S.phase = TurnPhase.waiting;
        cb = AiMiniMax(validCards);
        RemoveCard(cb);
        Bartok.S.MoveToTarget(cb);
        List<CardBartok> removeList = new List<CardBartok>();
        foreach (var card in hand)
        {
            if (card.rank == cb.rank)
            {
                card.faceUp = true;
                Bartok.S.MoveToDiscard(card);
                removeList.Add(card);
            }
        }
        if (removeList.Count != 0)
        {
            foreach (var item in removeList)
                RemoveCard(item);
        }
        cb.callbackPlayer = this;
    }
    CardBartok AiMiniMax(List<CardBartok> cards)
    {
        int i = 4;
        int _i = 0;
        CardBartok cb = null;
        if (cards.Count >= 4)
        {
            foreach (var card in cards)
            {
                int g = Count(card.rank);
                if (_i <= g)
                {
                    _i = g;
                    cb = card;
                }
            }
        }
        if (cards.Count < 4)
        {
            foreach (var card in cards)
            {
                int g = Count(card.rank);
                if (i >= g)
                {
                    i = g;
                    cb = card;
                }
            }
        }
        return cb;
    }
    int Count(int rank)
    {
        int i = 0;
        foreach (var card in hand)
            if (card.rank == rank)
                i++;
        return i;
    }
    public void CBCallback(CardBartok tCB)
    {
        Utils.tr("Player.CBCallback()", tCB.name, "Player " + playerNum);
        Bartok.S.PassTurn();
    }
}
