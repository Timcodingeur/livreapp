import express from "express";

import { booksRouter } from "./routes/books.mjs";

import { usersRouter } from "./routes/users.mjs";

import { sequelize, initDb } from "./db/sequelize.mjs";

import swaggerUi from "swagger-ui-express";

import { swaggerSpec } from "./swagger.mjs";

const app = express();

app.use(express.json());

const port = 3000;

app.use(
  "/api-docs",
  swaggerUi.serve,
  swaggerUi.setup(swaggerSpec, { explorer: true })
);

sequelize
  .authenticate()
  .then((_) =>
    console.log("La connexion à la base de donnée a bien été établie")
  )
  .catch((error) => console.error("Impossible de se connecter à la DB"));

initDb();

app.get("/", (req, res) => {
  res.send("API REST of self service machine !");
});

app.get("/", (req, res) => {
  res.redirect(`http://127.0.0.2:${port}`);
});

app.use("/api/books", booksRouter);

app.use("/api/users", usersRouter);

app.listen(port, () => {
  console.log(`Example app listening on port ${port}`);
});

app.use(({ res }) => {
  const message =
    "Impossible de trouver la ressource demandée ! Vous pouvez essayer une autre URL.";
  res.status(404).json(message);
});
