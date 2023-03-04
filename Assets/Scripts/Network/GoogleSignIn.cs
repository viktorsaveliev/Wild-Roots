/*using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase.Database;

public class GoogleSignIn : MonoBehaviour
{
    public Text nicknameField;
    public Text emailField;

    private FirebaseAuth auth;
    private DatabaseReference database;

    private void Awake()
    {
        // Initialize Firebase Authentication and Realtime Database
        auth = FirebaseAuth.DefaultInstance;
        database = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void SignInWithGoogle()
    {
        // Sign in with Google
        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        FirebaseUser user = auth.CurrentUser;

        if (user != null)
        {
            // The user is already signed in with Google
            SaveUserData(user.DisplayName, user.Email);
        }
        else
        {
            // The user is not signed in with Google, start the Google sign-in flow
            Credential credential = GoogleAuthProvider.GetCredential(null, null);
            auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("SignInWithCredentialAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("SignInWithCredentialAsync encountered an error: " + task.Exception);
                    return;
                }

                FirebaseUser newUser = task.Result;
                Debug.Log("User signed in successfully with Google account: " + newUser.DisplayName);

                // Save the user's nickname and email to the Realtime Database
                SaveUserData(newUser.DisplayName, newUser.Email);
            });
        }
    }

    private void SaveUserData(string nickname, string email)
    {
        // Save the user's nickname and email to the Realtime Database
        string userId = auth.CurrentUser.UserId;
        database.Child("users").Child(userId).Child("nickname").SetValueAsync(nickname);
        database.Child("users").Child(userId).Child("email").SetValueAsync(email);

        Debug.Log("User data saved to Firebase Realtime Database");
    }
}*/
