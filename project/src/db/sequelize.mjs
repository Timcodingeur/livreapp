import { Sequelize, DataTypes } from "sequelize";
import chokidar from "chokidar";
import EPub from "epub";
import fs from "fs";
import path from "path";

// Import des models
import { BooksModel } from "../models/books.mjs";
import { UserModel } from "../models/user.mjs";
import { users } from "./mock-users.mjs";

import bcrypt from "bcrypt";

const sequelize = new Sequelize("db_ouvrage", "root", "root", {
  host: "127.0.0.1",
  port: "6033",
  dialect: "mysql",
  logging: true,
});

const Book = BooksModel(sequelize, DataTypes);
const User = UserModel(sequelize, DataTypes);

let initDb = () => {
  return sequelize.sync({ force: true }).then((_) => {
    importUsers();
    watchEpubFiles();
    console.log("La base de données db_ouvrage a bien été synchronisée");
  });
};

const importUsers = () => {
  users.map((user) => {
    bcrypt.hash(user.password, 10).then((hash) => {
      User.create({
        username: user.username,
        password: hash,
        firstname: user.firstname,
        lastname: user.lastname,
      }).then((user) => console.log(user.toJSON()));
    });
  });
};

const watchEpubFiles = () => {
  console.log("1");
  const watcher = chokidar.watch("./src/db/books", {
    ignored: /(^|[\/\\])\../,
    ignoreInitial: false,
    persistent: true,
  });
  console.log(
    `Chemin absolu du répertoire surveillé : ${path.resolve("./src/db/books")}`
  );

  watcher.on("add", (filePath) => {
    console.log(`Fichier ajouté : ${filePath}`);
    processEpubFile(filePath);
  });
};

const processEpubFile = (filePath) => {
  console.log("yo");
  fs.readFile(filePath, (err, data) => {
    if (err) {
      console.error("Erreur lors de la lecture du fichier EPUB:", err);
      return;
    }

    const epub = new EPub(filePath, "/imagewebroot/", "/linkwebroot/");
    epub.on("end", function () {
      Book.create({
        title: epub.metadata.title,
        author: epub.metadata.creator,
        cover: "",
        content: data,
        description: epub.metadata.description || "No description available",
      })
        .then((book) => {
          console.log("Livre ajouté avec succès:", book.toJSON());
        })
        .catch((err) => {
          console.error(
            "Erreur lors de l'ajout du livre à la base de données:",
            err
          );
        });
    });

    epub.parse();
  });
};

export { sequelize, initDb, User, Book };
