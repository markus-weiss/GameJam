var Actuator = function (ui) {

    this.continueGame = function () {
        print("Actuator.continueGame");
    }
    
//    this.actuator.actuate(this.grid, {
//        score:      this.score,
//        over:       this.over,
//        won:        this.won,
//        bestScore:  this.storageManager.getBestScore(),
//        terminated: this.isGameTerminated()
//      });
  
    this.actuate = function (grid, metadata) {
        var self = this;
        
        self.clearContainer(self.tileContainer);
        
        grid.cells.forEach(function (column) {
            column.forEach(function (tile) {
                if (tile) {
                    self.addTile(tile);
                }
            });
        });
                
        self.updateScore(metadata.score);
        self.updateBestScore(metadata.bestScore);

        if (metadata.terminated) {
            if (metadata.over) {
                self.message(false); // You lose
            } 
            else if (metadata.won) {
                self.message(true); // You win!
            }
        }
    }
    
    this.clearContainer = function () {
        ui.clearUITiles();
    }
    
    this.addTile = function (tile) {
        var position  = tile.previousPosition || { x: tile.x, y: tile.y };
        
        var uiTile = ui.getUITile(tile.x, tile.y)        
        uiTile.setValue(tile.value);
        
        if (tile.previousPosition) {
            var previousUITile = ui.getUITile(tile.previousPosition.x, tile.previousPosition.y);
            uiTile.moveFrom(previousUITile.originPos);
        }
        
        else if (tile.mergedFrom) {
            tile.mergedFrom.forEach(function (merged) {
                //print("merged from (" + merged.x + "," + merged.y + ") -> (" + tile.x + "," + tile.y + ")");
                var tempUITile = ui.createTempUITile(tile.x, tile.y);
                tempUITile.setValue(merged.value);
                var previousUITile = ui.getUITile(merged.previousPosition.x, merged.previousPosition.y);
                tempUITile.moveFromTo(previousUITile.originPos, uiTile.originPos, true);
            });
            
            uiTile.playMergedAnim();
        }
        
        else {
            uiTile.playBornAnim();
        }
    }
    
    this.message = function (won) {
        //var type    = won ? "game-won" : "game-over";
        //var message = won ? "You win!" : "Game over!";

        ui.showResult(won);
    }
    
    this.continueGame = function () { 
        //print("continueGame"); 
    }
    
    var ls = null;
    this.updateScore = function (score) { 
        if (ls == null || ls != score) {
            if (ls != null) {
                var a = ui.oScore.GetComponent$1(UnityEngine.Animator.ctor);
                a.set_enabled(true);
                a.Play$$Int32(0);
            }

            ls = score;
            ui.oScore.set_text(score.toString());
        }
    }
    
    var lbs = null;
    this.updateBestScore = function (score) {
        if (lbs == null || lbs != score) {
            if (lbs != null) {
                var a = ui.oBestScore.GetComponent$1(UnityEngine.Animator.ctor);
                a.set_enabled(true);
                a.Play$$Int32(0);
            }

            lbs = score;
            ui.oBestScore.set_text(score.toString());
        }
    }
    
}    