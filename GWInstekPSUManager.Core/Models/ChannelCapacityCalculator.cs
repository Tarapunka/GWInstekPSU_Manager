namespace GWInstekPSUManager.Core.Models;

public class ChannelCapacityCalculator
{
    private DateTime _lastMeasurementTime;
    private double _accumulatedCapacityAh;
    private bool _firstMeasurement = true;

    public double CalculateCapacity(double currentA, DateTime currentTime)
    {
        if (_firstMeasurement)
        {
            _lastMeasurementTime = currentTime;
            _firstMeasurement = false;
            return 0;
        }

        // Вычисляем время прошедшее с последнего измерения в часах
        double elapsedHours = (currentTime - _lastMeasurementTime).TotalHours;
        _lastMeasurementTime = currentTime;

        // Интегрируем ток по времени (Ah = A * h)
        if (currentA > 0.01) // Учитываем только токи > 1 мА
        {
            _accumulatedCapacityAh += currentA * elapsedHours;
        }
        return _accumulatedCapacityAh;
    }

    public void Reset()
    {
        _accumulatedCapacityAh = 0;
        _firstMeasurement = true;
    }
}