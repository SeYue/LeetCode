using UnityEngine;

using System.Collections.Generic;

using UnityEditor;



public class Plugin_LoadingData : ScriptableWizard

{



    [MenuItem("GameObject/Data Setting/Loading text")]

    static void CreateWizard()

    {

        DisplayWizard<Plugin_LoadingData>("配置登陆提示文字", "确认", "取消");



    }



    // This is called when the user clicks on the Create button.  

    void OnWizardCreate()

    {

        Debug.Log("确认");

    }



    // Allows you to provide an action when the user clicks on the   

    // other button "Apply".  

    void OnWizardOtherButton()

    {

        Debug.Log("取消");



    }

}