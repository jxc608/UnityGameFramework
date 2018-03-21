using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoTest : UITest {

    [UITest]
    public IEnumerable GoToMap()
    {
        // Wait until object with given component appears in the scene
        yield return WaitFor(new SceneLoaded("StartScene"));

        // Wait until button with given name appears and simulate press event
        //yield return Press("BG");

        yield return WaitFor(new ObjectAppeared("LoadingSprite"));

        yield return new WaitForSeconds(.5f);
        Click();

        yield return WaitFor(new SceneLoaded("SelectLevelScene"));

        yield return Press("Level1");

        // Wait until Text component with given name appears and assert its value
        yield return WaitFor(new SceneLoaded("LevelMap3D"));

        //yield return Press("Button-Close");

        // Wait until object with given component disappears from the scene
        //yield return WaitFor(new ObjectDisappeared<SecondScreen>());
    }

   
}
