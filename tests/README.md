# Tests

## Side-by-side Tests

The `SideBySide` project is intended to verify that MySqlConnector doesn't break compatibility
with Connector/NET and that [known bugs have been fixed](https://mysqlconnector.net/tutorials/migrating-from-connector-net/#fixed-bugs).

The tests require a MySQL server. The simplest way to run one is with [Docker](https://www.docker.com/community-edition):

    docker run -d --rm --name mysqlconnector -e MYSQL_ROOT_PASSWORD=pass -p 3306:3306 mysql:8.0.27 --max-allowed-packet=96M --character-set-server=utf8mb4 --log-bin-trust-function-creators=1 --local-infile=1 --max-connections=250

Copy the file `SideBySide/config.json.example` to `SideBySide/config.json`, then edit
the `config.json` file in order to connect to your server. If you are using the Docker
command above, then the default options will work and do not need to be modified.
Otherwise, set the following options appropriately:

* `Data.ConnectionString`: The full connection string to your server. You should specify a database name. If the database does not exist, the test will attempt to create it.
* `Data.PasswordlessUser`: (Optional) A user account in your database with no password and no roles.
* `Data.SecondaryDatabase`: (Optional) A second database on your server that the test user has permission to access.
* `Data.CertificatesPath`: (Optional) The absolute path to the server and client certificates folder (i.e., the `.ci/server/certs` folder in this repo).
* `Data.MySqlBulkLoaderLocalCsvFile`: (Optional) The path to a test CSV file.
* `Data.MySqlBulkLoaderLocalTsvFile`: (Optional) The path to a test TSV file.
* `Data.UnsupportedFeatures`: A comma-delimited list of `ServerFeature` enum values that your test database server does *not* support
  * `CachingSha2Password`: a user named `caching-sha2-user` exists on your server and uses the `caching_sha2_password` auth plugin
  * `Ed25519`: a user named `ed25519user` exists on your server and uses the `client_ed25519` auth plugin
  * `ErrorCodes`: server returns error codes in error packet (some MySQL proxies do not)
  * `Json`: the `JSON` data type (MySQL 5.7 and later)
  * `LargePackets`: large packets (over 4MB)
  * `RoundDateTime`: server rounds `datetime` values to the specified precision (not implemented in MariaDB)
  * `RsaEncryption`: server supports RSA public key encryption (for `sha256_password` and `caching_sha2_password`)
  * `SessionTrack`: server supports `CLIENT_SESSION_TRACK` capability (MySQL 5.7 and later)
  * `Sha256Password`: a user named `sha256user` exists on your server and uses the `sha256_password` auth plugin
  * `StoredProcedures`: create and execute stored procedures
  * `Timeout`: server can cancel queries promptly (so timed tests don't time out)
  * `Tls11`: server supports TLS 1.1
  * `Tls12`: server supports TLS 1.2
  * `Tls13`: server supports TLS 1.3
  * `UnixDomainSocket`: server is accessible via a Unix domain socket
  * `UuidToBin`: server supports `UUID_TO_BIN` (MySQL 8.0 and later)

## Running Tests

There are two ways to run the tests: command line and Visual Studio.

### Visual Studio 2017

After building the solution, you should see a list of tests in the Test Explorer.  Click "Run All" to run them.

### Command Line

To run the tests against MySqlConnector:

```
cd tests\SideBySide
dotnet test -c Release
```

To run the tests against MySql.Data:

```
cd tests\SideBySide
dotnet restore /p:Configuration=Baseline && dotnet test -c Baseline
```

## User agreement

SINGLESTORE, INC. ("SINGLESTORE") AGREES TO GRANT YOU AND YOUR COMPANY ACCESS TO THIS OPEN SOURCE SOFTWARE CONNECTOR ONLY IF (A) YOU AND YOUR COMPANY REPRESENT AND WARRANT THAT YOU, ON BEHALF OF YOUR COMPANY, HAVE THE AUTHORITY TO LEGALLY BIND YOUR COMPANY AND (B) YOU, ON BEHALF OF YOUR COMPANY ACCEPT AND AGREE TO BE BOUND BY ALL OF THE OPEN SOURCE TERMS AND CONDITIONS APPLICABLE TO THIS OPEN SOURCE CONNECTOR AS SET FORTH BELOW (THIS “AGREEMENT”), WHICH SHALL BE DEFINITIVELY EVIDENCED BY ANY ONE OF THE FOLLOWING MEANS: YOU, ON BEHALF OF YOUR COMPANY, CLICKING THE “DOWNLOAD, “ACCEPTANCE” OR “CONTINUE” BUTTON, AS APPLICABLE OR COMPANY’S INSTALLATION, ACCESS OR USE OF THE OPEN SOURCE CONNECTOR AND SHALL BE EFFECTIVE ON THE EARLIER OF THE DATE ON WHICH THE DOWNLOAD, ACCESS, COPY OR INSTALL OF THE CONNECTOR OR USE ANY SERVICES (INCLUDING ANY UPDATES OR UPGRADES) PROVIDED BY SINGLESTORE.
BETA SOFTWARE CONNECTOR

Customer Understands and agrees that it is  being granted access to pre-release or “beta” versions of SingleStore’s open source software connector (“Beta Software Connector”) for the limited purposes of non-production testing and evaluation of such Beta Software Connector. Customer acknowledges that SingleStore shall have no obligation to release a generally available version of such Beta Software Connector or to provide support or warranty for such versions of the Beta Software Connector  for any production or non-evaluation use.

NOTWITHSTANDING ANYTHING TO THE CONTRARY IN ANY DOCUMENTATION,  AGREEMENT OR IN ANY ORDER DOCUMENT, SINGLESTORE WILL HAVE NO WARRANTY, INDEMNITY, SUPPORT, OR SERVICE LEVEL, OBLIGATIONS WITH
RESPECT TO THIS BETA SOFTWARE CONNECTOR (INCLUDING TOOLS AND UTILITIES).

APPLICABLE OPEN SOURCE LICENSE: MIT License

IF YOU OR YOUR COMPANY DO NOT AGREE TO THESE TERMS AND CONDITIONS, DO NOT CHECK THE ACCEPTANCE BOX, AND DO NOT DOWNLOAD, ACCESS, COPY, INSTALL OR USE THE SOFTWARE OR THE SERVICES.

