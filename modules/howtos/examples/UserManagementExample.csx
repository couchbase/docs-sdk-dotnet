// Run this using dotnet-script: https://github.com/filipw/dotnet-script
//
//      dotnet script UserManagementExample.csx
//

#r "nuget: CouchbaseNetClient, 3.2.0"

using System;
using System.Text;
using System.Threading.Tasks;
using Couchbase;
using Couchbase.Management.Users;
using Couchbase.Management.Query;
using Couchbase.Core.Exceptions;

await new UserManagementExample().ExampleAsync();

public class UserManagementExample
{
    private ICluster cluster {get; set; }
    private String bucketName = "travel-sample";
    private String testUsername = "cbtestuser";
    private String testPassword = "cbtestpassword";

    public async Task ExampleAsync()
    {
        cluster = await Cluster.ConnectAsync(
            "couchbase://your-ip",
            "Administrator", "password");

        await Example1();
        // await Example2();
        await Example3();
        await Example4();

        await cluster.DisposeAsync();
        Console.WriteLine("Done");
    }

    public async Task Example1() {
        // tag::usermanagement_1[]
        User user = new User(testUsername) {
            Password = testPassword,
            DisplayName = "Constance Lambert",
            Roles = new List<Role>() {
                // Roles required for the reading of data from the bucket
                new Role("data_reader", "*"),
                new Role("query_select", "*"),
                // Roles required for the writing of data into the bucket.
                new Role("data_writer", bucketName),
                new Role("query_insert", bucketName),
                new Role("query_delete", bucketName),
                // Role required for the creation of indexes on the bucket.
                new Role("query_manage_index", bucketName)
            }
        };

        await cluster.Users.UpsertUserAsync(user);
        // end::usermanagement_1[]
    }

    public async Task Example2() {
        // List current users.
        Console.WriteLine("Listing current users.");
        // tag::usermanagement_2[]
        IEnumerable<UserAndMetaData> listOfUsers = await cluster.Users.GetAllUsersAsync();

        foreach (UserAndMetaData currentUser in listOfUsers) {
            Console.WriteLine($"User's display name is: { currentUser.User().DisplayName }");
            IEnumerable<Role> currentRoles = currentUser.User().Roles;
            foreach (Role role in currentRoles) {
                Console.WriteLine($"   User has the role: { role.Name }, applicable to bucket { role.Bucket }");
            }
        }
        // end::usermanagement_2[]
    }

    public async Task Example3() {
        // Access the cluster that is running on the local host, specifying
        // the username and password already assigned by the administrator

        // tag::usermanagement_3[]
        var userCluster = await Cluster.ConnectAsync(
            "couchbase://your-ip",
            testUsername, testPassword);

        var userBucket = await userCluster.BucketAsync(bucketName);
        var scope = await userBucket.ScopeAsync("inventory");
        var collection = await scope.CollectionAsync("airline");

        try
        {
            await userCluster.QueryIndexes.CreatePrimaryIndexAsync(
                $"`{bucketName}`", // NCBC-2955
                new CreatePrimaryQueryIndexOptions().IgnoreIfExists(true));
        }
        catch (InternalServerFailureException)
        {
            Console.WriteLine("Primary index already exists!");
        }

        var returnedAirline10doc = await collection.GetAsync("airline_10");

        await collection.UpsertAsync(
            "airline_11", new {
                callsign = "MILE-AIR",
                iata = "Q5",
                icao = "MLA",
                id = 11,
                name = "40-Mile Air",
                type = "airline"
            }
        );

        var returnedAirline11Doc = await collection.GetAsync("airline_11");
        Console.WriteLine($"get -> { returnedAirline11Doc.ContentAs<dynamic>() }");

        var result = await userCluster.QueryAsync<dynamic>(
            "SELECT * FROM `travel-sample`.inventory.airline LIMIT 2");

        Console.WriteLine("query ->");
        await foreach (var airline in result.Rows)
        {
            Console.WriteLine(airline);
        }

        await userCluster.DisposeAsync();
        // end::usermanagement_3[]
    }

    public async Task Example4() {
        // tag::usermanagement_4[]
        await cluster.Users.DropUserAsync(testUsername);
        // end::usermanagement_4[]
        await cluster.DisposeAsync();
    }
}
