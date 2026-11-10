//-----------------------------------Examen Aimar Sáez ACCESO A DATOS-----------------------------------//

//---------------- INICIO ----------------
using System.Text;

string rutaCsv = "acb2526.csv";
List<Partido> partidos = LeerPartidos(rutaCsv);

//---------------- MENU ----------------

int opcion;
do{
    Console.Clear();
    Console.WriteLine("------ GESTIÓN DE PARTIDOS ACB 25-26 ------");
    Console.WriteLine("1. Resultados de un equipo");
    Console.WriteLine("2. Resultados de una jornada");
    Console.WriteLine("3. Top 3 MVPs");
    Console.WriteLine("4. Clasificación");
    Console.WriteLine("5. Guardar todos los resultados en ficheros por equipo");
    Console.WriteLine("6. Salir");
    Console.Write("Elige una opción: ");
    if (!int.TryParse(Console.ReadLine(), out opcion)){
        opcion = 0;
    }
    switch (opcion){
        case 1:
            MostrarResultadosEquipo(partidos);
            break;
        case 2:
            MostrarResultadosJornada(partidos);
            break;
        case 3:
            MostrarTopMVPs(partidos);
            break;
        case 4:
            MostrarClasificacion(partidos);
            break;
        case 5:
            GenerarFicherosEquipos(partidos);
            break;
        default:
            if (opcion != 6)
            {
                Console.WriteLine("Opción no válida.");
            }
            break;
    }

    if (opcion != 6)
    {
        Console.WriteLine("\nPulsa cualquier tecla para volver al menú...");
        Console.ReadKey();
    }
} while (opcion != 6);


//---------------- METODOS ----------------

//METODO RESULTADOS DE UN EQUIPO

void MostrarResultadosEquipo(List<Partido> lista){
    Console.Write("Introduce el nombre del equipo (ej: Bilbao Basket) es importante que pongas el nombre correctamente: ");
    string equipo = Console.ReadLine().Trim();
    List<Partido> resultados = new List<Partido>();
    foreach (Partido p in partidos){
        if (string.Equals(p.EquipoLocal, equipo, StringComparison.OrdinalIgnoreCase) || string.Equals(p.EquipoVisitante, equipo, StringComparison.OrdinalIgnoreCase)){
            resultados.Add(p);
        }
    }

    if (resultados.Count == 0){
        Console.WriteLine("No se han encontrado resultados para ese equipo.");
        return;
    }

    string paraTXT = equipo + "\n";
    Console.WriteLine(paraTXT);
    
    foreach (var p in resultados){
        string lineaPartido = $"Jornada {p.Jornada}: {p.EquipoLocal} {p.PuntosLocal} - {p.PuntosVisitante} {p.EquipoVisitante} (MVP: {p.MVP} - {p.PuntosMVP} pts, {p.AsistenciasMVP} ast, {p.RebotesMVP} reb, {p.EquipoMVP})\n";        Console.WriteLine(lineaPartido);
        paraTXT += lineaPartido;
    }

    PreguntarGuardado(paraTXT);
}

//METODO RESULTADOS DE UNA JORNADA

void MostrarResultadosJornada(List<Partido> lista){
    Console.Write("Introduce el número de la jornada: ");
    string textoJornada = Console.ReadLine().Trim();
    if (!int.TryParse(textoJornada, out int jornada))
    {
        Console.WriteLine("Entrada inválida. Por favor, introduce un número.");
        return;
    }

    List<Partido> resultados = new List<Partido>();
    foreach (Partido p in lista){
        if (p.Jornada == jornada){
            resultados.Add(p);
        }
    }
    if (resultados.Count == 0){
        Console.WriteLine("No hay datos para esa jornada.");
        return;
    }

    string paraTXT = jornada + "\n";
    Console.WriteLine(paraTXT);
    foreach (var p in resultados){
        string lineaPartido = $"{p.EquipoLocal} {p.PuntosLocal} - {p.PuntosVisitante} {p.EquipoVisitante} (MVP: {p.MVP} - {p.PuntosMVP} pts, {p.AsistenciasMVP} ast, {p.RebotesMVP} reb, {p.EquipoMVP})\n";
        Console.WriteLine(lineaPartido);
        paraTXT += lineaPartido;
    }

    PreguntarGuardado(paraTXT);
}

//METODO TOP3 MVP

void MostrarTopMVPs(List<Partido> lista){
    Dictionary<string, int> conteoMVPs = new Dictionary<string, int>();

    foreach (Partido p in lista){
        if (conteoMVPs.ContainsKey(p.MVP)){
            conteoMVPs[p.MVP]++;
        }
        else{
            conteoMVPs.Add(p.MVP, 1);
        }
    }

    var mvpsOrdenados = conteoMVPs.OrderByDescending(numOrden => numOrden.Value).ToList();

    string paraTXT = "------ TOP 3 MVPs ------\n";
    Console.WriteLine(paraTXT);
    
    for (int i = 0; i < 3 ; i++){
        string linea = $"{i+1}. {mvpsOrdenados[i].Key} - {mvpsOrdenados[i].Value} veces\n";
        Console.WriteLine(linea);
        paraTXT += linea;
    }

    PreguntarGuardado(paraTXT);
}

//METODO CLASIFICACION

void MostrarClasificacion(List<Partido> lista){
    var estadisticas = new Dictionary<string, EquipoStats>();

    foreach (var p in lista){
        estadisticas.TryAdd(p.EquipoLocal, new EquipoStats(p.EquipoLocal));
        estadisticas.TryAdd(p.EquipoVisitante, new EquipoStats(p.EquipoVisitante));

        if (p.PuntosLocal > p.PuntosVisitante){
            estadisticas[p.EquipoLocal].Victorias++;
            estadisticas[p.EquipoVisitante].Derrotas++;
        }
        else if (p.PuntosVisitante > p.PuntosLocal){
            estadisticas[p.EquipoVisitante].Victorias++;
            estadisticas[p.EquipoLocal].Derrotas++;
        }
    }

    List<EquipoStats> clasificacion = new List<EquipoStats>(estadisticas.Values);

    // Criterio en caso de empate: más victorias primero. Si empatan, gana el enfrentamiento directo.
    for (int i = 0; i < clasificacion.Count - 1; i++){
        for (int j = i + 1; j < clasificacion.Count; j++){
            EquipoStats a = clasificacion[i];
            EquipoStats b = clasificacion[j];

            bool intercambiar = false;

            if (b.Victorias > a.Victorias){
                intercambiar = true;
            }
            else if (b.Victorias == a.Victorias){
                int ganaA = VictoriasContra(lista, a.Nombre, b.Nombre);
                int ganaB = VictoriasContra(lista, b.Nombre, a.Nombre);

                if (ganaB > ganaA){
                    intercambiar = true;
                }  
            }

            if (intercambiar){
                clasificacion[i] = b;
                clasificacion[j] = a;
            }
        }
    }

    string paraTXT = "------ CLASIFICACIÓN ACB ------\n";
    Console.WriteLine(paraTXT);
    Console.WriteLine(String.Format("{0,-5} | {1,-34} | {2,-5} | {3,-5}", "POS", "EQUIPO", "VIC", "DER"));
    
    for (int i = 0; i < clasificacion.Count; i++){
        var equipo = clasificacion[i];
        string linea = String.Format("{0,-5} | {1,-34} | {2,-5} | {3,-5}", i + 1, equipo.Nombre, equipo.Victorias, equipo.Derrotas);
        Console.WriteLine(linea);
        paraTXT += linea + "\n";
    }
}

//METODO GENERAR FICHEROS POR CADA EQUIPO

void GenerarFicherosEquipos(List<Partido> lista){
    List<string> equipos = new List<string>(); 
    foreach (var p in lista)
    {
        if (!equipos.Contains(p.EquipoLocal)){
            equipos.Add(p.EquipoLocal);
        }

        if (!equipos.Contains(p.EquipoVisitante)){
            equipos.Add(p.EquipoVisitante);
        }
    }

    int contador = 0;
    foreach (var equipo in equipos){
        var partidosEquipo = lista.Where(p => p.EquipoLocal == equipo || p.EquipoVisitante == equipo).ToList();
        string texto = equipo + "\n\n";  
        Console.WriteLine($"------ RESULTADOS HISTÓRICOS: {equipo} ------");
        foreach (var p in partidosEquipo){
            texto += $"J{p.Jornada}: {p.EquipoLocal} {p.PuntosLocal} - {p.PuntosVisitante} {p.EquipoVisitante} (MVP: {p.MVP} - {p.PuntosMVP} pts, {p.AsistenciasMVP} ast, {p.RebotesMVP} reb, {p.EquipoMVP})\n";
        }
        string nombreFichero = $"{equipo.Replace(" ", "_")}.txt";
        File.WriteAllText(nombreFichero, texto, Encoding.UTF8);
        contador++;
    }
    Console.WriteLine($"Se han generado correctamente {contador} ficheros de equipos en la carpeta del programa.");
}

//---------------- METODOS SECUNDARIOS ----------------

void PreguntarGuardado(string contenido){
    Console.Write("\n¿Quieres guardar esta información en un fichero .txt? (s/n): ");
    string respuesta = Console.ReadLine().ToLower();
    
    if (respuesta == "s"){
        Console.Write("Introduce el nombre del fichero (sin extensión): ");
        string nombreFichero = Console.ReadLine().Trim();
        
        while (string.IsNullOrWhiteSpace(nombreFichero)){
            Console.WriteLine("No has escrito ningún nombre. Inténtalo de nuevo.");
            Console.Write("Introduce el nombre del fichero: ");
            nombreFichero = Console.ReadLine();
        }
        string rutaCompleta = nombreFichero + ".txt";
        try{
            File.WriteAllText(rutaCompleta, contenido, Encoding.UTF8);
            Console.WriteLine($"Datos guardados correctamente en {rutaCompleta}");
        }
        catch (Exception ex){
            Console.WriteLine($"Error al guardar el fichero: {ex.Message}");
        }
    }
}

//METODO PARA COMPROBAR VICTORIAS ENTRE DOS EQUIPOS

int VictoriasContra(List<Partido> lista, string a, string b)
{
    int victorias = 0;
    foreach (Partido p in lista)
    {
        if (p.EquipoLocal == a && p.EquipoVisitante == b && p.PuntosLocal > p.PuntosVisitante)
        {
            victorias++;
        }
        else if (p.EquipoLocal == b && p.EquipoVisitante == a && p.PuntosVisitante > p.PuntosLocal)
        {
            victorias++;
        }
    }
    return victorias;
}

//METODO PARA LEER CSV Y CREAR LISTA DE PARTIDOS

List<Partido> LeerPartidos(string ruta){
    var lista = new List<Partido>();
    string[] lineas = File.ReadAllLines(ruta, Encoding.UTF8);
    for (int i = 1; i < lineas.Length; i++)
    {
        string[] datos = lineas[i].Split(','); 
        if (datos.Length >= 9)
        {
            string textoJornada = datos[0].Replace("Jornada", "").Trim();
            int jornada = int.Parse(textoJornada);            
            string local = datos[1].Trim();
            string visitante = datos[2].Trim();

            string[] puntos = datos[3].Split('-');
            int ptsLocal = int.Parse(puntos[0]);
            int ptsVisita = int.Parse(puntos[1]);
            
            string mvp = datos[4].Trim();
            int ptsMVP = int.Parse(datos[5]);
            int astMVP = int.Parse(datos[6]);
            int rebMVP = int.Parse(datos[7]);
            string equipoMVP = datos[8].Trim();
            lista.Add(new Partido(jornada, local, visitante, ptsLocal, ptsVisita, mvp, ptsMVP, astMVP, rebMVP, equipoMVP));
        }
    }
    return lista;
}

//---------------- CLASES ---------------- 
public record Partido(
    int Jornada, string EquipoLocal, string EquipoVisitante, int PuntosLocal, int PuntosVisitante, string MVP,int PuntosMVP,int AsistenciasMVP,int RebotesMVP,string EquipoMVP
);
class EquipoStats{
    public string Nombre { get; set; }
    public int Victorias { get; set; } = 0;
    public int Derrotas { get; set; } = 0;

    public EquipoStats(string nombre){
        Nombre = nombre;
    }
}