using TMPro;
using Unity.AppUI.UI;
using UnityEngine;

public class ReinforceDisplayResult : UI_Base
{
    public enum TMPs
    {
        ExtraDamage_TMP,
        ExtraCriDamage_TMP,
        ExtraCriChance_TMP,
        ExtraDefense_TMP,
        ExtraHealth_TMP,
        ExtraMana_TMP,
        ExtraLevel_TMP,
        Result_TMP,
        END
    }
   private bool _hasInitialized = false;
    private TextMeshProUGUI[] _texts = new TextMeshProUGUI[(int)TMPs.END];

    public override void Init() 
    {
        if (true ==_hasInitialized)
            return;
        _hasInitialized = true;

        Bind<TextMeshProUGUI>(typeof(TMPs));
        _texts[(int)TMPs.ExtraDamage_TMP] =Get<TextMeshProUGUI>((int)TMPs.ExtraDamage_TMP);
        _texts[(int)TMPs.ExtraCriDamage_TMP] =Get<TextMeshProUGUI>((int)TMPs.ExtraCriDamage_TMP);
        _texts[(int)TMPs.ExtraCriChance_TMP] =Get<TextMeshProUGUI>((int)TMPs.ExtraCriChance_TMP);
        _texts[(int)TMPs.ExtraDefense_TMP] =Get<TextMeshProUGUI>((int)TMPs.ExtraDefense_TMP);
        _texts[(int)TMPs.ExtraHealth_TMP] =Get<TextMeshProUGUI>((int)TMPs.ExtraHealth_TMP);
        _texts[(int)TMPs.ExtraMana_TMP] =Get<TextMeshProUGUI>((int)TMPs.ExtraMana_TMP);
        _texts[(int)TMPs.ExtraLevel_TMP] =Get<TextMeshProUGUI>((int)TMPs.ExtraLevel_TMP);
        _texts[(int)TMPs.Result_TMP] =Get<TextMeshProUGUI>((int)TMPs.Result_TMP);
        foreach (var text in _texts)
        {
            text.text ="";
        }
    }
    public void SetResult (Reinforce.ReinforceResult result)
    {


        _texts[(int)TMPs.ExtraDamage_TMP].text =  "추가 공격력: + " +result.Damage.ToString();
        _texts[(int)TMPs.ExtraCriDamage_TMP].text ="추가 치명타: +" + result.CriDamage.ToString();
        _texts[(int)TMPs.ExtraCriChance_TMP].text ="추가 치명타 확률: +" + result.CriChance.ToString();
        _texts[(int)TMPs.ExtraDefense_TMP].text = "추가 방어력: +" +result.Defense.ToString();
        _texts[(int)TMPs.ExtraHealth_TMP].text = "추가 체력: + " +result.MaxHP.ToString();
        _texts[(int)TMPs.ExtraMana_TMP].text = "추가 마나: +" +result.MaxMP.ToString();
        _texts[(int)TMPs.ExtraLevel_TMP].text = "강화 레벨: " +result.ReinforceLevel.ToString();
        if (true ==result.bReinforceResult)
        {
            _texts[(int)TMPs.Result_TMP].text = "강화 성공";
            _texts[(int)TMPs.Result_TMP].color = Color.yellow;
        }
        else
        {
            _texts[(int)TMPs.Result_TMP].text = "강화 실패";
            _texts[(int)TMPs.Result_TMP].color = Color.red;
        }

    }

    public void ClearTexts()
    {
        foreach(var text in _texts)
        {
            text.text = "";
        }
    }
   
}
