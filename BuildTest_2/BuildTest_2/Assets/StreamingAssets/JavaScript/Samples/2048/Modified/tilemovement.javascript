jss.define_mb("TileMovement", function () {
    this.Awake = function() {
        this.trans = this.get_transform();
    }
    
    this.moving = false;
    
    var $cv = { Value: UnityEngine.Vector3.get_zero() };
    this.Update = function () {
        if (!this.moving) {
            return;
        }
    
        var newPos = UnityEngine.Vector3.SmoothDamp$$Vector3$$Vector3$$Vector3$$Single(
                this.trans.get_position(),
                this.toPos,
                $cv,
                0.07);

        this.trans.set_position(newPos);

        var dis = UnityEngine.Vector3.Distance(newPos, this.toPos);
        if (dis < 0.001) {
            this.moving = false;

            if (this.destroyAfterFinish) {
                UnityEngine.Object.Destroy$$Object(this.get_gameObject());
            }
        }
    }
    
    this.moveFromTo = function (fromPos, toPos, destroyAfterFinish) {
        this.toPos = toPos;
        this.destroyAfterFinish = destroyAfterFinish;
        this.moving = true;
        this.trans.set_position(fromPos);
    }
});