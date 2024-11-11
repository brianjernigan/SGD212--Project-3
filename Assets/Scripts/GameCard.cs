using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCard
{
    public CardData Data { get; private set; }
    public CardUI UI { get; private set; }

    public GameCard(CardData data, CardUI ui)
    {
        Data = data;
        UI = ui;
    }
}
