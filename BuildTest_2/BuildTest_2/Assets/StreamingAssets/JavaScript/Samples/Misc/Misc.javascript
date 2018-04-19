jss.define_mb("Dog", function () {
    this.Eat = function () {
        print("EAT!");
    }
    
    this.OnGUI = function () {
        //print("on gui");
    }
});

jss.define_mb("Misc", function() {
    this.oDogGameObject = null;

    this.Start = function () {
        var dog = this.oDogGameObject.GetComponent$1(jss.Dog);
        dog.Eat();

        this.oDogGameObject.AddComponent$1(jss.Dog, 1/* Custom JSComponent */ );

        //this.StartCoroutine$$IEnumerator(this.SayHello());
    }

    this.Update = function () {
        if (UnityEngine.Input.GetKeyDown$$KeyCode(UnityEngine.KeyCode.UpArrow)) {
            //
            // var go = new UnityEngine.GameObject.ctor$$String("Created by Misc");
            //var go = new UnityEngine.GameObject.ctor();
            //go.set_name("create by misc 2");

            var go = this.get_gameObject();
            //var trans = go.GetComponent$1(UnityEngine.Transform.ctor);
            //print(trans.get_rotation().x);

            var image = go.AddComponent$1(UnityEngine.UI.Image.ctor);
            image.set_color(new UnityEngine.Color.ctor$$Single$$Single$$Single(125/255, 78/255, 3/255));
        }

        jss.UpdateCoroutineAndInvokes(this);
    }

    this.SayHello = function*() {
        var i = 10;

        while (i > 0) {
            print("hello jsb " + i);
            i--;
            yield new UE.WaitForSeconds.ctor(1);
        }
    }
});