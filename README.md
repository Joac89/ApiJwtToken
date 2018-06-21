# Api en .Net Core 2 implementando JWT Token
El siguiente ejercicio, nos muestra como implementar JWT Token en un proyecto Web Api en .Net Core 2, como base inicial para lograr una autenticación al API por medio de un token generado.

## Tabla de contenido
1. [Concepto](#concepto)
2. [Archivo appsettings](#archivo-appsettings)
2. [Archivo Startup](#archivo-startup)
3. [Token Controller](#token-controller)
4. [Validando el modelo](#validando-el-modelo)
5. [Probando el API](#probando-el-api)
6. [Conclusión](#conclusion)

## Concepto
(Tomado de: [¿Qué es Json Web Token (JWT)?](https://www.programacion.com.py/varios/que-es-json-web-token-jwt))

"Json Web Token es un conjunto de medios de seguridad para peticiones http y así representar demandas para ser transferidos entre dos partes (cliente y servidor). Las partes de un JWT se codifican como un objeto JSON que está firmado digitalmente utilizando JSON Web Signature( JWS ).

![JWT TOKEN](https://www.programacion.com.py/wp-content/uploads/2016/07/autenticacion-basada-en-token.png)

La mayoría de las aplicaciones actuales consumen servicios rest y están alojadas en distintos dominios con lo cuál no podemos trabajar con sesiones ya que se almacenan en este.

Podemos decir que la mejor alternativa es llevar a cabo la autenticación haciendo uso de tokens que vayan del servidor al cliente, un usuario hace login (no necesita enviar token porque no lo tiene), una vez el servidor de ok retorna un token cómo respuesta y el usuario debe enviar dicho token en las siguientes peticiones para poder acceder a los recursos del servicio.

En cada petición el servidor debe comprobar el token proporcionado por el usuario y si es correcto podrá acceder a los recursos solicitados, de otra forma deberá denegar la petición."

## Archivo Appsettings
Ahora, luego de la explicación referenciada en el apartado anterior, procedemos a implementar el uso de JWT para el API. Para ello, primero configuramos el archivo **appsettings.json** del proyecto, que almacenará las variables necesarias en la utilización de JWT.

El archivo **appsettings.json** original al crear un proyecto Web Api con .Net Core 2 en Visual Studio 2017 es el siguiente:
```
{
  "Logging": {
    "IncludeScopes": false,
    "Debug": {
      "LogLevel": {
        "Default": "Warning"
      }
    },
    "Console": {
      "LogLevel": {
        "Default": "Warning"
      }
    }
  }
}
```
Añadiremos el siguiente código que contiene las variables necesarias para JWT.
```
{
  "token": {
    "issuer": "example.token.issuer",
    "audience": "example.token.audience",
    "signingKey": "example.toke.key",
    "expire": 8
  },
  "connectionString": "ServerDataBaseConnectionString"
}
```
El nuevo archivo **appsettings.json** de nuestro proyecto quedaría de la siguiente manera:
```
{
  "Logging": {
    "IncludeScopes": false,
    "Debug": {
      "LogLevel": {
        "Default": "Warning"
      }
    },
    "Console": {
      "LogLevel": {
        "Default": "Warning"
      }
    }
  },
  "token": {
    "issuer": "example.token.issuer",
    "audience": "example.token.audience",
    "signingKey": "example.toke.key",
    "expire": 8
  }
}

```
La definición para los campos JWT se describe a continuación:
1. issuer: Identifica el proveedor de identidad que emitió el JWT
2. audience: Identifica la audiencia o receptores para lo que el JWT fue emitido. Cada servicio que recibe un JWT para su validación tiene que controlar la audiencia a la que el JWT está destinado. Si el proveedor del servicio no se encuentra presente en el campo aud, entonces el JWT tiene que ser rechazado
3. expire: Identifica la marca temporal luego de la cual el JWT no tiene que ser aceptado. 
4. signkey: Llave de seguridad para el cifrado

[estandar JWT](https://tools.ietf.org/html/rfc7519#section-4.1.3)

## Archivo Startup
Luego de configurar el archivo **appsettings.json**, continuamos con el ajuste al archivo **startup.cs** para la implementación de JWT. Para ello, en el método **ConfigureServices** se implementa en la colección de servicios a utilizar, la autenticación con JWT Token.

El código inicial de **startup** es el siguiente:
```
public void ConfigureServices(IServiceCollection services)
{
	services.AddMvc();
}
```

La implementación para JWT en **startup.cs** quedaría de la siguiente manera:
```
public void ConfigureServices(IServiceCollection services)
{
	services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
		 .AddJwtBearer(options =>
		 {
			 options.TokenValidationParameters =
				  new TokenValidationParameters
				  {
					  ValidateIssuer = true,
					  ValidateAudience = true,
					  ValidateLifetime = true,
					  ValidateIssuerSigningKey = true,

					  ValidIssuer = Configuration["token:issuer"],
					  ValidAudience = Configuration["token:audience"],
					  IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["token:signingkey"]))
				  };
		 });
	services.AddMvc();
}
```
Las variables como **issuer** que aparecen en el código, se toman desde el archivo **appsettings.json** configurado anteriormente. Con ésto, tendríamos implementado la autenticación JWT para cada controlador que vayamos incluyendo en el proyecto por medio del atributo  **Authorize**.


## Token Controller
Luego de configurar el archivo **appsettings.json** y el archivo **startup.cs** del proyecto, pasamos a construir el controlador al que se le pedirá el token de autenticación JWT. La implementación es similar a la usada en el archivo **startup.cs**

La declaración del controlador se implementa de la siguiente manera:
```
[AllowAnonymous]
[HttpPost]
public async Task<IActionResult> GetToken([FromBody] AuthEntity data)
{
  ...
}
```
El controlador, cuenta con un método HTTP-POST que recibe una entidad tipo AuthEntity, compuesta por User y Password y devuelve un objeto con el token generado que se explicará mas adelante.

Una vez declarado el controlador, se procede a implementar la utilización de JWT TOKEN. Las variables como **issuer** que aparecen en el código, se toman desde el archivo **appsettings.json**
```
[AllowAnonymous]
[HttpPost]
public async Task<IActionResult> GetToken([FromBody] AuthEntity data)
{  
  var claims = new[]
	{
		new Claim(JwtRegisteredClaimNames.Sub, data.UserName),
		new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
	};

	var token = new JwtSecurityToken
	(
		 issuer: config["token:issuer"],
		 audience: config["token:audience"],
		 claims: claims,
		 expires: DateTime.UtcNow.AddHours(double.Parse(config["token:expire"])),
		 notBefore: DateTime.UtcNow,
		 signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["token:signingkey"])),  SecurityAlgorithms.HmacSha256)
	);

	var token = new JwtSecurityTokenHandler().WriteToken(token);
	return Ok(token);
}
```

Ahora, podemos mejorar el controlador para que pueda validar el usuario con algún repositorio de datos. Para eso, ajustamos el código del controlador de la siguiente manera, agregando las líneas de codigo siguientes.
```
var result = new ResponseBase<string>();
var userAuth = new ResponseBase<bool>();

userAuth = await new ExampleDao().SimulateLogin(data.UserName, data.Password);
```
La clase **ResponseBase** (que se encuentra en el proyecto), sirve para generalizar las respuestas de los controladores. A futuro, todos los controladores podrían usarla para poder tener una misma respuesta en todos.
Finalmente, se agrega una nueva línea de código para validar la autenticación correcta del usuario:
```
if (userAuth.Code == 200 && userAuth.Data)
{
  ...
}
```

El código final del controlador quedaría de la siguiente manera:
```
[AllowAnonymous]
[HttpPost]
public async Task<IActionResult> GetToken([FromBody] AuthEntity data)
{
  var result = new ResponseBase<string>();
	var userAuth = new ResponseBase<bool>();

	//Escriba aquí la implementación para validar usuario y contraseña de acceso
	userAuth = await new ExampleDao().SimulateLogin(data.UserName, data.Password);

	if (userAuth.Code == 200 && userAuth.Data)
	{
		var claims = new[]
		{
			new Claim(JwtRegisteredClaimNames.Sub, data.UserName),
			new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
		};

	  var token = new JwtSecurityToken
	  (
		  issuer: config["token:issuer"],
		  audience: config["token:audience"],
		  claims: claims,
		  expires: DateTime.UtcNow.AddHours(double.Parse(config["token:expire"])),
		  notBefore: DateTime.UtcNow,
		  signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["token:signingkey"])),  SecurityAlgorithms.HmacSha256)
	  );

	  result.Code = (int)HttpStatusCode.OK;
	  result.Data = new JwtSecurityTokenHandler().WriteToken(token);
	  result.Message = userAuth.Message;

	  return Ok(result);
  }
  else
  {
		  result.Code = (int)HttpStatusCode.Unauthorized;
		  result.Message = userAuth.Message;
		  result.Data = "";

		  return StatusCode(result.Code, result);
  }
}
```

## Validando el Modelo
Ya tenemos el controlador que contiene el método para obtener un token JWT desde el API y poder validarse. Ahora, necesitamos validar el modelo de datos de entrada para el método. Para ello, utilizamos un filtro de atributos que nos permitirá verificar de manera mas sencilla que el modelo de datos de entrada es correcto es decir, si es requerido o no, si necesita cumplir con requisitos como longitud, expresiones regulares, entre otros.

Lo único que debemos hacer es en el archivo **ValidateModelAttribute.cs** del proyecto, reescribir el método **OnActionExecuting** y codificar la respuesta en caso de que la validación del modelo falle.

El código del método a reescribir es el siguiente:
```
public override void OnActionExecuting(ActionExecutingContext actionContext)
{
	if (!actionContext.ModelState.IsValid)
	{
		var result = new ResponseBase<BadRequestObjectResult>()
		{
			Code = (int)HttpStatusCode.BadRequest,
			Data = new BadRequestObjectResult(actionContext.ModelState),
			Message = "Invalid Model"
		};
	actionContext.Result = new BadRequestObjectResult(result);
	}
}
```
En la implementación de validación, se pregunta si el estado del modelo en el contexto actual es válido. En caso de que no lo sea, se devuelve un objeto de tipo **ResponseBase** indicando el error al validar el modelo como resultado de respuesta del controlador.

Ejemplo de controlador para validar el modelo con el atributo **ValidateModel**:
```
[AllowAnonymous]
[HttpPost]
[ValidateModel]
public async Task<IActionResult> GetToken([FromBody] AuthEntity data)
{
  ...
}
```

## Probando el API
Podemos probar el API, creando un controlador de ejemplo que implemente la autenticación por JWT.
```
namespace ApiJwtTokenExample.Controllers
{
	[Produces("application/json")]
	[Route("api/v1/[controller]")]
	public class IndexController : Controller
  {
		private IConfiguration config;
		public IndexController(IConfiguration configuration)
		{
			config = configuration;
		}

		[Authorize]
		[HttpGet]
		[Route("validate")]
		[ValidateModel]
		public IActionResult Index()
    {
        var result = new ResponseBase<bool>() {
				  Code = 200,
				  Message = "Example API jwt Token"
	      };

			  return Ok(result);
    }
  }
}
```
En el controlador de ejemplo, utilizamos el atributo **ValidateModel** y **Authorize** que nos permitiran validar el modelo de datos de entrada y el token válido enviado en los encabezados de la petición HTTP. En caso de que no sea válido el Token, nos devolverá un 401 indicando que no se ha autorizado al usuario para realizar la consulta solicitada.

### Usando el API desde POSTMAN ###
Solicitud de Token:

Solicitud al controlador de ejemplo con OK:

Solicitud al controlador de ejemplo con 401 (No autorizado):



## Conclusión
