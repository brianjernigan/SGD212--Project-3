public class KrakenEffect : ICardEffect
{
    public string EffectDescription => "No effect. This card can be played as any rank.";

    public void ActivateEffect()
    {
        var shellyController = GameManager.Instance.ShellyController;
        shellyController.ActivateTextBox("Krakens have no effects by themselves!");
    }
}
