﻿using System;
using System.Collections.Generic;
using System.Threading;

namespace ChessClockApp
{
    public enum Player
    {
        ONE,
        TWO
    }

    public abstract class ChessClock : IDisposable
    {
        private readonly TimeSpan _gameTime;

        private Timer _countdownTimer;
        private DateTime? _lastTimerStart;
        private Player? _currentPlayer;

        protected readonly Dictionary<Player, TimeSpan> _remainingTime = new Dictionary<Player, TimeSpan>(2)
        {
            { Player.ONE, TimeSpan.Zero },
            { Player.TWO, TimeSpan.Zero }
        };

        protected TimeSpan TimerElapsedTime => _lastTimerStart.HasValue ?
            DateTime.Now - _lastTimerStart.Value : TimeSpan.Zero;

        public ChessClock(TimeSpan gameTime)
        {
            _gameTime = gameTime;
            _remainingTime[Player.ONE] = gameTime;
            _remainingTime[Player.TWO] = gameTime;
            _countdownTimer = new Timer(TimeUp);
            _countdownTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void PressButton(Player player)
        {
            if (ChangePlayer(player))
            {
                UpdatePlayerRemainingTime(player);
                RestartTimer();
            }
        }

        public TimeSpan GetRemainingTime(Player player)
        {
            var elapsedTime = TimeSpan.Zero;
            if (player == _currentPlayer)
                elapsedTime = TimerElapsedTime;
            return _remainingTime[player] - elapsedTime;
        }

        public void Reset()
        {
            _remainingTime[Player.ONE] = _gameTime;
            _remainingTime[Player.TWO] = _gameTime;
            _currentPlayer = null;
            _lastTimerStart = null;
            _countdownTimer.Dispose();
            _countdownTimer = new Timer(TimeUp);
            _countdownTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private bool ChangePlayer(Player from)
        {
            var otherPlayer = from == Player.ONE ? Player.TWO : Player.ONE;
            if (_currentPlayer == otherPlayer)
                return false;

            _currentPlayer = otherPlayer;
            return true;
        }

        private void RestartTimer()
        {
            _countdownTimer.Dispose();
            _countdownTimer = new Timer(TimeUp);
            _countdownTimer.Change(_remainingTime[_currentPlayer.Value], TimeSpan.FromMilliseconds(-1));
            _lastTimerStart = DateTime.Now;
        }

        private void TimeUp(object state)
        {
            _remainingTime[_currentPlayer.Value] = TimeSpan.Zero;
            _currentPlayer = null;
            _lastTimerStart = null;
            _countdownTimer.Dispose();
            _countdownTimer = new Timer(TimeUp);
            _countdownTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void Dispose() => _countdownTimer.Dispose();
        protected abstract void UpdatePlayerRemainingTime(Player player);
    }
}