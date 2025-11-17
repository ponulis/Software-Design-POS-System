import { useEffect, useState } from "react";

function App() {
  const [weather, setWeather] = useState([]);

  useEffect(() => {
    fetch("http://localhost:5168/weatherforecast")
      .then(res => res.json())
      .then(data => setWeather(data))
      .catch(err => console.error(err));
  }, []);

  return (
    <div>
      <h1>Weather forecast:</h1>
      <p>
        {weather.map((w, index) => (
          <li key={index}>
            {w.date} – {w.summary} – {w.temperatureC}°C
          </li>
        ))}
      </p>
    </div>
  );
}

export default App;