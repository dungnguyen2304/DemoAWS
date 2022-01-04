using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

// Manages all the text and button inputs
// Also acts like the main manager script for the game.
public class UIInputManager : MonoBehaviour
{
   public static string CachePath;

   public Button btnSignup;
   public Button btnLogin;
   public Button btnStart;
   public Button btnLogout;
   public InputField ifEmailLogin;
   public InputField ifPasswordFieldLogin;
   public InputField ifUsername;
   public InputField ifEmail;
   public InputField ifPassword;
   public GameObject _unauthInterface;
   public GameObject _authInterface;
   public LambdaManager _lambdaManager;
   public GameObject _loading;
   public GameObject _welcome;
   public GameObject _confirmEmail;
   public GameObject _signupContainer;
    public GameObject _subSignup;
    public GameObject _loginContainer;

   private AuthenticationManager _authenticationManager;
   private List<Selectable> _fields;
   private int _selectedFieldIndex = -1;

    

    private void displayComponentsFromAuthStatus(bool authStatus)
   {
      if (authStatus)
      {
         // Debug.Log("User authenticated, show welcome screen with options");
         _loading.SetActive(false);
         _unauthInterface.SetActive(false);
         _authInterface.SetActive(true);
         _welcome.SetActive(true);
      }
      else
      {
         // Debug.Log("User not authenticated, activate/stay on login scene");
         _loading.SetActive(false);
         _unauthInterface.SetActive(true);
         _authInterface.SetActive(false);
      }

      // clear out passwords
      ifPasswordFieldLogin.text = "";
      ifPassword.text = "";

      // set focus to email field on login form
      _selectedFieldIndex = -1;
   }

   private async void onLoginClicked()
   {
      _unauthInterface.SetActive(false);
      _loading.SetActive(true);
      // Debug.Log("onLoginClicked: " + emailFieldLogin.text + ", " + passwordFieldLogin.text);
      bool successfulLogin = await _authenticationManager.Login(ifEmailLogin.text, ifPasswordFieldLogin.text);
      displayComponentsFromAuthStatus(successfulLogin);
   }

   private async void onSignupClicked()
   {
      _unauthInterface.SetActive(false);
      _loading.SetActive(true);

      // Debug.Log("onSignupClicked: " + usernameField.text + ", " + emailField.text + ", " + passwordField.text);
      bool successfulSignup = await _authenticationManager.Signup(ifUsername.text, ifEmail.text, ifPassword.text);

      if (successfulSignup)
      {
         // here we re-enable the whole auth container but hide the sign up panel
         //_signupContainer.SetActive(false);

         _confirmEmail.SetActive(true);

         // copy over the new credentials to make the process smoother
         ifEmailLogin.text = ifEmail.text;
         ifPasswordFieldLogin.text = ifPassword.text;

         // set focus to email field on login form
         _selectedFieldIndex = 0;
      }
      else
      {
         _confirmEmail.SetActive(false);

         // set focus to email field on signup form
         _selectedFieldIndex = 3;
      }

      _loading.SetActive(false);
      _unauthInterface.SetActive(true);
   }

   private void onLogoutClick()
   {
      _authenticationManager.SignOut();
      displayComponentsFromAuthStatus(false);
   }

   private void onStartClick()
   {
      SceneManager.LoadScene("GameScene");
      Debug.Log("Changed to Playmode");

      // call to lambda to demonstrate use of credentials
      _lambdaManager.ExecuteLambda();
   }

   private async void RefreshToken()
   {
      bool successfulRefresh = await _authenticationManager.RefreshSession();
      displayComponentsFromAuthStatus(successfulRefresh);
   }

   void Start()
   {
      Debug.Log("UIInputManager: Start");
      // check if user is already authenticated 
      // We perform the refresh here to keep our user's session alive so they don't have to keep logging in.
      RefreshToken();

      btnSignup.onClick.AddListener(onSignupClicked);
      btnLogin.onClick.AddListener(onLoginClicked);
      btnStart.onClick.AddListener(onStartClick);
        btnLogout.onClick.AddListener(onLogoutClick);
   }



   void Awake()
   {
      CachePath = Application.persistentDataPath;
      _unauthInterface.SetActive(false); 
      _authInterface.SetActive(false);
      _welcome.SetActive(false);
      _confirmEmail.SetActive(false);
      _signupContainer.SetActive(false);

      _authenticationManager = FindObjectOfType<AuthenticationManager>();
      _lambdaManager = FindObjectOfType<LambdaManager>();

      _fields = new List<Selectable> { ifEmailLogin, ifPasswordFieldLogin, btnLogin, ifEmail, ifUsername, ifPassword, btnSignup };
   }
}
