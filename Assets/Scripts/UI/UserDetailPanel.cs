using UnityEngine;

public class UserDetailPanel : UI_Panel
{
    public string CurrentUserId { get; private set; }

    public void ShowUser(string userId)
    {
        CurrentUserId = userId;
    }
}
