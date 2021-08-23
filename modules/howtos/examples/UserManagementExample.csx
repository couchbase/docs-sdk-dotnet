// Run this using dotnet-script: https://github.com/filipw/dotnet-script
//
//      dotnet script UserManagementExample.cs
//

#r "nuget: CouchbaseNetClient, 3.2.0"

using System;
using System.Text;
using System.Threading.Tasks;
using Couchbase;
using Couchbase.Management.Users;

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
            "couchbase://localhost",
            "Administrator", "password");


        
        await Example1();
        // await Example2();
        
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
}

