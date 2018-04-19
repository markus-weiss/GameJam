
var UnityStorageManager = function () {
    var bestScoreKey = "2048BestScroe";
    var gameStateKey = "2048GameState";

    this.getBestScore = function () {
        var bs = UnityEngine.PlayerPrefs.GetInt$$String$$Int32(bestScoreKey, 0);
        return bs;
    }
    this.setBestScore = function (bestScore) {
        UnityEngine.PlayerPrefs.SetInt(bestScoreKey, bestScore);
    }


    this.clearGameState = function () {
        UnityEngine.PlayerPrefs.SetString(gameStateKey, "");
    }
    this.getGameState = function () {
        var gameState = UnityEngine.PlayerPrefs.GetString$$String$$String(gameStateKey, "");
        if (gameState != null && gameState.length > 0) {
            return JSON.parse(gameState);
        }
    }
    this.setGameState = function (gameState) {
        UnityEngine.PlayerPrefs.SetString(gameStateKey, JSON.stringify(gameState));
    }
}