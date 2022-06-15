using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRTK;

public class CharacterSelector : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] Button button_female;
    [SerializeField] Button button_male;
    [SerializeField] Button button_confirm;

    [Header("Avatars")]
    [SerializeField] GameObject avatar_female;
    [SerializeField] GameObject avatar_male;

    [Header("Settings")]
    [SerializeField] float approxAvatarEyeLevel = 1.72f;
    [SerializeField] GameObject ground;

    private VRTK_HeadsetFade headsetFade;
    private Transform headsetTransform;

    private Image img_female;
    private Color color_img_female;
    private Image img_male;
    private Color color_img_male;

    public enum AvatarGender
    {
        FEMALE,
        MALE,
    }

    private AvatarGender currentGender;

    private struct AvatarOption
    {
        private readonly GameObject _avatar;
        public GameObject Avatar { get { return _avatar; } }

        private readonly AvatarGender _gender;
        public AvatarGender Gender { get { return _gender; } }

        private readonly Button _button;
        public Button Button { get { return _button; } }

        public AvatarOption(GameObject avatar, AvatarGender gender, Button button)
        {
            this._avatar = avatar;
            this._gender = gender;
            this._button = button;
        }
    }

    private AvatarOption option_female;
    private AvatarOption option_male;
    private List<AvatarOption> avatarOptions = new List<AvatarOption>();

    private GameController gameController;

    void Start()
    {
        gameController = GameController.Instance;

        option_female = new AvatarOption(avatar_female, AvatarGender.FEMALE, button_female);
        option_male = new AvatarOption(avatar_male, AvatarGender.MALE, button_male);
        avatarOptions.Add(option_female);
        avatarOptions.Add(option_male);

        button_male.onClick.AddListener(ButtonMalePressed);
        button_female.onClick.AddListener(ButtonFemalePressed);
        button_confirm.onClick.AddListener(ConfirmButtonPressed);

        img_female = button_female.gameObject.transform.parent.GetComponent<Image>();
        color_img_female = img_female.color;

        img_male = button_male.gameObject.transform.parent.GetComponent<Image>();
        color_img_male = img_male.color;

        DisplayRandomAvatarOnStart();
    }

    /// <summary>
    /// Randomly displays one of the avatar options at start
    /// </summary>
    private void DisplayRandomAvatarOnStart()
    {
        int randomIndex = Random.Range(0, avatarOptions.Count);
        ToggleButtonColors(avatarOptions[randomIndex].Button);
        SwapActiveAvatar(avatarOptions[randomIndex].Avatar);
    }

    /// <summary>
    /// Highlights the female avatar selection button and enables the female avatar
    /// </summary>
    private void ButtonFemalePressed()
    {
        ToggleButtonColors(option_female.Button);
        SwapActiveAvatar(option_female.Avatar);
    }

    /// <summary>
    /// Highlights the male avatar selection button and enables the male avatar
    /// </summary>
    private void ButtonMalePressed()
    {
        ToggleButtonColors(option_male.Button);
        SwapActiveAvatar(option_male.Avatar);
    }

    /// <summary>
    /// Reduces the alpha value for the parent panel of the unselected button to better visualize the selection.
    /// </summary>
    /// <param name="pressed"> Button which was just pressed by the player </param>
    private void ToggleButtonColors(Button pressed)
    {
        if (pressed == button_female)
        {
            img_female.color = new Color(color_img_female.r, color_img_female.g, color_img_female.b, 1f);
            img_male.color = new Color(color_img_male.r, color_img_male.g, color_img_male.b, 0.47f);
        }
        else if (pressed == button_male)
        {
            img_male.color = new Color(color_img_male.r, color_img_male.g, color_img_male.b, 1f);
            img_female.color = new Color(color_img_female.r, color_img_female.g, color_img_female.b, 0.47f);
        }
    }

    /// <summary>
    /// Swaps the displayed avatar and sets the current gender accordingly
    /// </summary>
    /// <param name="avatar"> Avatar which will be enabled </param>
    private void SwapActiveAvatar(GameObject avatar)
    {
        if (avatar == option_female.Avatar)
        {
            option_female.Avatar.SetActive(true);
            option_male.Avatar.SetActive(false);
            currentGender = option_female.Gender;
        }
        else if (avatar == option_male.Avatar)
        {
            option_male.Avatar.SetActive(true);
            option_female.Avatar.SetActive(false);
            currentGender = option_male.Gender;
        }
    }

    /// <summary>
    /// Calls the calculation of the avatar scaling factor
    /// </summary>
    private void ConfirmButtonPressed()
    {
        CalculateScalingFactor();
    }

    /// <summary>
    /// Calculates a character scaling based on the players height, measured via a raycast from the eye level to the scene floor.
    /// Additionally, starts player relocation process to start the game
    /// </summary>
    private void CalculateScalingFactor()
    {
        if (VRTK_DeviceFinder.HeadsetCamera() != null)
        {
            headsetTransform = VRTK_DeviceFinder.HeadsetCamera().transform;
            headsetFade = VRTK_DeviceFinder.HeadsetCamera().gameObject.GetComponent<VRTK_HeadsetFade>();
        }

        Ray ray = new Ray(headsetTransform.position, -Vector3.up);
        RaycastHit hit;

        ground.GetComponent<BoxCollider>().Raycast(ray, out hit, 100f);

        string scalingFactor = (hit.distance / approxAvatarEyeLevel).ToString("F2");
        Debug.Log($"Headset Transform = {headsetTransform.position} || Ray Length = {hit.distance} \n || Scaling Factor = {scalingFactor}");

        gameController.SaveAvatarConfig(currentGender, float.Parse(scalingFactor));

        StartCoroutine(gameController.FadeHeadset());
    }
}
