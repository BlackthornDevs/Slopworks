using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveControllerBehaviour : MonoBehaviour
{
    [SerializeField] private EnemySpawner _spawner;
    [SerializeField] private List<WaveDefinition> _waves;
    [SerializeField] private GameEventSO _waveStartedEvent;
    [SerializeField] private GameEventSO _waveEndedEvent;

    private WaveController _controller;
    private ThreatMeter _threat;

    public WaveController Controller => _controller;
    public ThreatMeter Threat => _threat;

    private void Awake()
    {
        _threat = new ThreatMeter();
        _controller = new WaveController(_waves, _threat);

        _controller.OnWaveStarted += HandleWaveStarted;
        _controller.OnWaveEnded += HandleWaveEnded;
    }

    private void OnDestroy()
    {
        _controller.OnWaveStarted -= HandleWaveStarted;
        _controller.OnWaveEnded -= HandleWaveEnded;
    }

    public void BeginNextWave()
    {
        var def = _controller.StartNextWave();
        if (def == null)
        {
            Debug.Log("all waves complete");
            return;
        }

        StartCoroutine(SpawnWaveCoroutine(def));
    }

    public void ReportEnemyKilled()
    {
        _controller.OnEnemyKilled();
    }

    private IEnumerator SpawnWaveCoroutine(WaveDefinition def)
    {
        int spawnCount = _controller.EnemiesRemaining;

        for (int i = 0; i < spawnCount; i++)
        {
            if (_spawner != null)
                _spawner.SpawnWave(1);

            if (def.spawnDelay > 0f && i < spawnCount - 1)
                yield return new WaitForSeconds(def.spawnDelay);
        }
    }

    private void HandleWaveStarted()
    {
        if (_waveStartedEvent != null)
            _waveStartedEvent.Raise();

        Debug.Log("wave " + (_controller.CurrentWave + 1) + " started — " +
                  _controller.EnemiesRemaining + " enemies");
    }

    private void HandleWaveEnded()
    {
        if (_waveEndedEvent != null)
            _waveEndedEvent.Raise();

        Debug.Log("wave " + (_controller.CurrentWave + 1) + " cleared");

        // auto-start next wave after rest period if waves remain
        if (_controller.CurrentWave + 1 < _controller.TotalWaves)
        {
            float rest = _waves[_controller.CurrentWave].timeBetweenWaves;
            StartCoroutine(RestThenNextWave(rest));
        }
    }

    private IEnumerator RestThenNextWave(float delay)
    {
        yield return new WaitForSeconds(delay);
        BeginNextWave();
    }
}
