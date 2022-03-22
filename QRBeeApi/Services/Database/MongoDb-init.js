//sh.createDatabase("QRBee");

//db["Users"].drop();
//db["Certificates"].drop();
//db["Transactions"].drop();

db.createCollection("Users");
db.createCollection("Certificates");
db.createCollection("Transactions");

db["Users"].createIndex(
  {
      "_id": 1
  }
);
db["Users"].createIndex(
  {
      "Email": 1
  },
  {
      unique: false,
      sparse: true
  }
);
db["Certificates"].createIndex(
  {
      "_id": 1
  }
);
db["Certificates"].createIndex(
  {
      "ClientId": 1
  },
  {
      unique: true,
      sparse: true
  }
);
db["Transactions"].createIndex(
  {
      "_id": 1
  }
);
