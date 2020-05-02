using Api.Library.Models;
using NUnit.Framework;
using System;
using System.Text.Json;

namespace Api.Tests
{
    public class JsonDeserializationTests
    {
        [Test]
        public void UserInfo_should_deserialize_DateTime_correctly()
        {
            var json = @"{ ""updated_at"":""2020-04-05T20:20:20.000Z"" }";
            var obj = JsonSerializer.Deserialize<UserInfo>(json);
            var expected = new DateTime(2020, 04, 05, 20, 20, 20);

            Assert.That(obj, Is.Not.Null);
            Assert.That(obj.UpdatedAt, Is.Not.EqualTo(DateTime.MinValue));
            Assert.That(obj.UpdatedAt, Is.EqualTo(expected));
        }

        [Test]
        public void UserInfo_shoulddeserialize_correctly()
        {
            var json = @"{""sub"":""auth0 | 5e89f436447efd0be07453f9"",
""nickname"":""gjdass"",
""name"":""gjdass @yopmail.fr"",
""picture"":""https://coucou.coucou"",
""updated_at"":""2020-05-01T23:54:29.000Z"",
""email"":""gjdass@yopmail.fr"",
""email_verified"":true}";
            var obj = JsonSerializer.Deserialize<UserInfo>(json);

            Assert.That(obj, Is.Not.Null);
            Assert.That(obj.Nickname, Is.EqualTo("gjdass"));
            Assert.That(obj.Name, Is.EqualTo("gjdass @yopmail.fr"));
            Assert.That(obj.Picture, Is.EqualTo("https://coucou.coucou"));
            Assert.That(obj.UpdatedAt, Is.EqualTo(new DateTime(2020, 05, 01, 23, 54, 29)));
            Assert.That(obj.Email, Is.EqualTo("gjdass@yopmail.fr"));
            Assert.That(obj.EmailVerified, Is.True);
        }
    }
}