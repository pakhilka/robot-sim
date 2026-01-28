namespace RobotSim.Interfaces
{
    /// <summary>
    /// Интерфейс для всех датчиков робота
    /// </summary>
    public interface ISensor
    {
        /// <summary>
        /// Уникальный идентификатор датчика
        /// </summary>
        string SensorId { get; }

        /// <summary>
        /// Инициализировать датчик
        /// </summary>
        void Initialize();

        /// <summary>
        /// Получить значение датчика
        /// </summary>
        object GetValue();
    }
}
