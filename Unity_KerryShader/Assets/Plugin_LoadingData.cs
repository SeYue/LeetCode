using UnityEngine;

using System.Collections.Generic;

using UnityEditor;



public class Plugin_LoadingData : ScriptableWizard

{



    [MenuItem("GameObject/Data Setting/Loading text")]

    static void CreateWizard()

    {

        DisplayWizard<Plugin_LoadingData>("���õ�½��ʾ����", "ȷ��", "ȡ��");



    }



    // This is called when the user clicks on the Create button.  

    void OnWizardCreate()

    {

        Debug.Log("ȷ��");

    }



    // Allows you to provide an action when the user clicks on the   

    // other button "Apply".  

    void OnWizardOtherButton()

    {

        Debug.Log("ȡ��");



    }

}