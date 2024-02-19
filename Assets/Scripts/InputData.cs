using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;

public class InputData : MonoBehaviour
{
    public TMP_InputField nameField;
    public TMP_InputField ageField;
    public TMP_Dropdown genderDropdown;
    public Text warning;
    public CSVData csvData;

    public void SaveData()
    {
        // Check if all fields have valid values
        if (string.IsNullOrEmpty(nameField.text) || string.IsNullOrEmpty(ageField.text))
        {
            warning.text = "Please fill in all fields!!";
            Debug.LogWarning("Please fill in all fields!");
            return;
        }

        int age = int.Parse(ageField.text);

        // Assign the CSV file path to the ScriptableObject
        csvData.csvFilePath = Application.dataPath + "/Data/" + nameField.text + ".csv";

        // Write the data to the CSV file
        using (StreamWriter sw = File.AppendText(csvData.csvFilePath))
        {
            sw.WriteLine("Name," + nameField.text);
            sw.WriteLine("Age," + age);
            sw.WriteLine("Gender," + genderDropdown.options[genderDropdown.value].text);
        }

        Debug.Log("Data saved to: " + csvData.csvFilePath);
        SceneManager.LoadScene("Level_1");
    }

    public void SkipData()
    {
        csvData.csvFilePath = Application.dataPath + "/Data/Test.csv";
        using (StreamWriter sw = File.AppendText(csvData.csvFilePath))
        {
            sw.WriteLine("Name," + "TEST");
            sw.WriteLine("Age," + "TEST");
            sw.WriteLine("Gender," + "TEST");
        }

        Debug.Log("Data saved to: " + csvData.csvFilePath);
        SceneManager.LoadScene("Level_1");
    }

}
